import sys
import logging
import os
import stat
import shutil
import zipfile
import win32file
import kyofilesigner.utilities.constants as CONST
from datetime import datetime
from uuid import uuid4
from subprocess import Popen, PIPE, STDOUT
logging.basicConfig(stream=sys.stdout, level=logging.INFO)

def lprint(message):
    logging.info(f"""

    ----------------------------------------
    {message}
    ----------------------------------------
    
    """)

# From: https://stackoverflow.com/a/2656408
def rmtree(top, retain_top=False):
    """
    A safer implementation of shutil's rmtree
    shutil's rmtree cannot delete read only files
    """
    for root, dirs, files in os.walk(top, topdown=False):
        for name in files:
            filename = os.path.join(root, name)
            os.chmod(filename, stat.S_IWRITE)
            os.remove(filename)
        for name in dirs:
            d = os.path.join(root, name)
            os.chmod(d, stat.S_IWRITE)
            os.rmdir(d)
    
    if not retain_top and os.path.exists(top):
        os.rmdir(top)

def on_rm_error(func, path, exc_info):
    # path contains the path of the file that couldn't be removed
    # let's just assume that it's read-only and unlink it.
    if os.path.exists(path):
        win32file.SetFileAttributes(path, win32file.FILE_ATTRIBUTE_READONLY)
        os.remove(path)

def change_permissions(top, mode):
    for root, dirs, files in os.walk(top, topdown=False):
        for dir in [os.path.join(root,d) for d in dirs]:
            os.chmod(dir, mode)
    for _file in [os.path.join(root, f) for f in files]:
            os.chmod(_file, mode)

def move(src, dest):
    # delete file first if there is
    if os.path.exists(dest):
        if os.path.isfile(dest):
            os.remove(dest)
        elif os.path.isdir(dest) and os.path.isdir(src):
            rmtree(dest)
        elif os.path.isdir(dest) and os.path.isfile(src) and os.path.exists(os.path.join(dest, os.path.split(src)[1])):
            os.remove(os.path.join(dest, os.path.split(src)[1]))

    shutil.move(src, dest)

# https://gist.github.com/dreikanter/5650973
def copydir(source, dest, indent = 0):
    """Copy a directory structure overwriting existing files"""
    for root, dirs, files in os.walk(source):
        if not os.path.isdir(root):
            os.makedirs(root)
        for each_file in files:
            rel_path = root.replace(source, '').lstrip(os.sep)
            dest_path = os.path.join(dest, rel_path, each_file)
            os.makedirs(os.path.dirname(dest_path), exist_ok=True)
            shutil.copyfile(os.path.join(root, each_file), dest_path)

def copytree(src, dest):
    # delete folder first if there is
    if os.path.exists(dest):
        rmtree(dest)
    
    shutil.copytree(src, dest)

def create_log_file(build_params, name):
    """
    Create's a log file...
    """
    path = os.path.join(os.path.expandvars(CONST.LOGSFOLDER), build_params.name, build_params.version)
    fname = os.path.join(path, name)

    # Create directory
    os.makedirs(path, exist_ok=True)
    # Create file
    open(fname, "w").close()

    return fname

def run_shell_command(command, cwd="", log_file=None, append=False, no_print=False, return_output=False):
    """
    Runs a command in the shell
    """
    print(f"Running command: {command} in {cwd}")
    if not cwd:
        cwd = os.getcwd()

    # If no log file, then just execute like this
    if not log_file:
        if not no_print:
            p = Popen(command, shell=True, cwd=cwd)
        else:
            p = Popen(command, stdout=PIPE, shell=True, cwd=cwd)
        out = p.communicate()[0]

        if p.returncode != 0 and p.returncode != None:
            raise Exception(f"Error in executing: {command}    ::: Return code is {p.returncode}")

        if return_output:
            return out.decode()
            
        return p.returncode

    # Remove log file first if not appending
    if not append:
        os.remove(log_file)
        
    with Popen(command, stdout=PIPE, stderr=STDOUT, bufsize=1, shell=True, cwd=cwd) as p, open(log_file, "ab") as f:
        for line in p.stdout: # b'\n'-separated lines
            if not no_print:
                sys.stdout.buffer.write(line) # pass bytes as is
            f.write(line)

        if p.returncode != 0 and p.returncode != None:
            raise Exception(f"Error in executing: {command}    ::: Return code is {p.returncode}")

        return p.returncode

def make_archive(source, destination, exclude_root=False):
    """
    Create's an archive.
    """
    # Remove destination if it exists
    if os.path.exists(destination):
        os.remove(destination)

    if os.path.exists(source):
        zip_file = zipfile.ZipFile(destination, "w", zipfile.ZIP_DEFLATED)

        # The root directory within the ZIP file.
        rootdir = os.path.basename(source)

        for dirpath, dirnames, filenames in os.walk(source): # pylint: disable=unused-variable
            # if exclude_root:
            #     dirpath = os.path.basename(source[source_length:])
            for filename in filenames:
                # Write the file named filename to the archive,
                # giving it the archive name 'arcname'.
                filepath = os.path.join(dirpath, filename)
                parentpath = os.path.relpath(filepath, source)

                # remove the root folder name if exclude_root is True
                if exclude_root:
                    arcname = parentpath
                else:
                    arcname = os.path.join(rootdir, parentpath)

                zip_file.write(filepath, arcname)

    zip_file.close()

    return destination

def generate_unique_id():
    """
    Creates a unique ID by using time and guid
    """
    return datetime.now().strftime("%Y%m-%d%H-%M%S-") + str(uuid4())