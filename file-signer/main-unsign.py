import sys
import os
import traceback
import argsparser
import glob
import logging
import shutil
import stat
from datetime import datetime
from kyofilesigner.filesigner import FileSigner
from kyofilesigner.catfilegenerator import CatFileGenerator
import kyofilesigner.utilities.common as COMMON

def retrieve_signable_files(folder):
    pattern_inner_dll = os.path.join(folder, "**", "*.dll")
    pattern_inner_sys = os.path.join(folder, "**", "*.sys")
    pattern_inner_exe = os.path.join(folder, "**", "*.exe")
    pattern_inner_ps1 = os.path.join(folder, "**", "*.ps1")
    pattern_dll = os.path.join(folder, "*.dll")
    pattern_sys = os.path.join(folder, "*.sys")
    pattern_exe = os.path.join(folder, "*.exe")
    pattern_ps1 = os.path.join(folder, "*.ps1")
    files = glob.glob(pattern_dll) + glob.glob(pattern_sys) + glob.glob(pattern_exe) + glob.glob(pattern_ps1) + glob.glob(pattern_inner_dll, recursive=True) + glob.glob(pattern_inner_sys, recursive=True) + glob.glob(pattern_inner_exe, recursive=True) + glob.glob(pattern_inner_ps1, recursive=True)
    # Return unique list
    return list(set(files))

def retrieve_cat_files(folder):
    pattern_inner_cat = os.path.join(folder, "**", "*.cat")
    pattern_cat = os.path.join(folder, "*.cat")
    files = glob.glob(pattern_cat) + glob.glob(pattern_inner_cat, recursive=True)
    # Return unique list
    return list(set(files))

def remove_readonly(arg, dirname, files):
    for f in files:
        filename = os.path.join(dirname, f)
        if not os.access(filename, os.W_OK):
            os.chmod(filename, stat.S_IWRITE)

try:
    # Parse the arguments
    args = argsparser.parse()

    # Start the time!
    start_time = datetime.now()

    # Let's do this
    # Check if main folder exists
    if not os.path.exists(args.main_folder):
        logging.info(f"Source does not exist: {args.main_folder}")
        sys.exit(1)

    # Remove that readonly
    os.walk(args.main_folder, remove_readonly, None)

    # Delete the bare and the target folder
    temporary_target_folder = args.main_folder + "_bare"
    if os.path.exists(temporary_target_folder):
        # Remove the readonly if there is
        os.walk(temporary_target_folder, remove_readonly, None)
        # Delete the folder
        shutil.rmtree(temporary_target_folder, COMMON.on_rm_error)
    if os.path.exists(args.target):
        # Remove the readonly if there is
        os.walk(args.target, remove_readonly, None)
        # Delete the folder
        shutil.rmtree(args.target, COMMON.on_rm_error)

    # Our heroes, initialize
    tools_location = os.path.join(os.path.dirname(os.path.realpath(__file__)), "kyofilesigner", "tools")
    filesigner = FileSigner(os.path.join(tools_location, "signtool", "signtool.exe"), os.path.join(tools_location, "certificate.pfx"))
    catfilegenerator = CatFileGenerator(os.path.join(tools_location, "inf2cat", "inf2cat.exe"))

    # Enumerate all the sources and targets
    # Start with the main folder
    # If there are "Included folders", then we will just process those in included folders
    # If none then we will process everything in the main folder
    signable_files = []
    # This is somewhat hacky, if there are no included folders then we will add the base one
    real_included_folders = args.included_folders
    if len(args.included_folders) <= 0:
        args.included_folders = [""]

    for folder in args.included_folders:
        source_folder = os.path.abspath(os.path.join(args.main_folder, folder))
        target_folder = os.path.abspath(os.path.join(temporary_target_folder, folder))

        # Copy the source to target
        os.makedirs(target_folder, exist_ok=True)
        os.walk(target_folder, remove_readonly, None)
        COMMON.copydir(source_folder, target_folder)

        # Get all the files to sign
        signable_files += retrieve_signable_files(target_folder)

    # Variable for holding the signed files
    processed_files = []

    for _file in signable_files:
        # Check if the file is signed
        if filesigner.verify(_file):
            logging.info(f"File is signed: {_file} unsigning it")
            filesigner.unsign(_file)
            processed_files.append(_file)

    # Copy the main folder to the target folder IF there is a passed included folder
    if len(real_included_folders) > 0:
        COMMON.copydir(args.main_folder, args.target)

    # Move the temporary target folders to the real one
    for folder in args.included_folders:
        temp_target_folder = os.path.abspath(os.path.join(temporary_target_folder, folder))
        target_folder = os.path.abspath(os.path.join(args.target, folder))
        COMMON.copydir(temp_target_folder, target_folder)

    # Print the processed files
    COMMON.lprint("List of files that are generated/unsigned")
    for _file in processed_files:
        logging.info(_file)

    # End the time! And then computed end time
    COMMON.lprint("Finished generating, check runtime below")
    end_time = datetime.now()
    start_time_str = start_time.strftime("%B %d, %Y %I:%M:%S %p")
    end_time_str = end_time.strftime("%B %d, %Y %I:%M:%S %p")
    running_time = (datetime.min + (end_time - start_time)).time()
    running_time_str = running_time.strftime("%H:%M:%S")
    print(f"\nStart time: {start_time_str}")
    print(f"End time: {end_time_str}")
    print(f"Run time: {running_time_str}")

    # This is a marker that states that the build is really done, don't change
    print("\n\nDONE BUILDING")
except Exception:
    print("Unexpected error:", traceback.format_exc())
    print("\n\nERROR ERROR ERROR")
    sys.exit(1)
finally:
    # Clean up
    shutil.rmtree(temporary_target_folder, COMMON.on_rm_error)