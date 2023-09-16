from bz2 import compress
import sys
import os
import stat
import traceback
import argsparser
import glob
import logging
from datetime import datetime
from joblib import Parallel, delayed, cpu_count
from kyofilesigner.filesigner import FileSigner
from kyofilesigner.catfilegenerator import CatFileGenerator
from kyofilesigner.compressor.compressor import Compressor
import kyofilesigner.utilities.common as COMMON

# Change logging info
logging.basicConfig(stream=sys.stdout, level=logging.INFO)

# Our heroes, initialize
tools_location = os.path.join(os.path.dirname(os.path.realpath(__file__)), "kyofilesigner", "tools")
filesigner = FileSigner(os.path.join(tools_location, "signtool", "signtool.exe"), os.path.join(tools_location, "certificate.pfx"))
catfilegenerator = CatFileGenerator(os.path.join(tools_location, "inf2cat", "inf2cat.exe"))
compressor = Compressor()

# Flag for stopping
STOP = False

def retrieve_signable_files(folder):
    pattern_inner_dl_ = os.path.join(folder, "**", "*.dl_")
    pattern_inner_dll = os.path.join(folder, "**", "*.dll")
    pattern_inner_sys = os.path.join(folder, "**", "*.sys")
    pattern_inner_exe = os.path.join(folder, "**", "*.exe")
    pattern_inner_msi = os.path.join(folder, "**", "*.msi")
    pattern_inner_ps1 = os.path.join(folder, "**", "*.ps1")
    pattern_dl_ = os.path.join(folder, "*.dl_")
    pattern_dll = os.path.join(folder, "*.dll")
    pattern_sys = os.path.join(folder, "*.sys")
    pattern_exe = os.path.join(folder, "*.exe")
    pattern_msi = os.path.join(folder, "*.msi")
    pattern_ps1 = os.path.join(folder, "*.ps1")
    files = glob.glob(pattern_dl_) + glob.glob(pattern_dll) + glob.glob(pattern_sys) + glob.glob(pattern_exe) + glob.glob(pattern_msi) + glob.glob(pattern_ps1) + glob.glob(pattern_inner_dl_, recursive=True) + glob.glob(pattern_inner_dll, recursive=True) + glob.glob(pattern_inner_sys, recursive=True) + glob.glob(pattern_inner_exe, recursive=True) + glob.glob(pattern_inner_msi, recursive=True) + glob.glob(pattern_inner_ps1, recursive=True)
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

def process_folder(folder):
    COMMON.lprint(f"Processing folder: {folder}")
    
    processed = []
    # Get all the files to sign
    signable_files = retrieve_signable_files(folder)
    processed += sign_list(signable_files)

    # Get all the cat files
    cat_files = retrieve_cat_files(folder)
    processed += generate_cats(cat_files)
    
    return processed

def sign_list(signable_files):
    processed = []

    for _file in signable_files:
        if STOP:
            raise CancelException

        is_compressed = False

        # Check if file is compressed
        if _file[-1] == "_":
            # Uncompress it
            # If filename is dl_ then
            target_name = ""
            if _file.endswith("dl_"):
                target_name = _file[:-1] + "l"
            _file = compressor.uncompress_single(_file,  True, target_name)
            is_compressed = True

        COMMON.lprint(f"Signing file: {_file}")
        # Check if the file is already signed
        if filesigner.verify(_file):
            logging.info(f"File is already signed: {_file} unsigning first")
            filesigner.unsign(_file)

        # Sign it now
        if filesigner.sign(_file):
            # If file is compressed then compress it back
            if is_compressed:
                _file = compressor.compress_single(_file, True)

            processed.append(_file)

    return processed

def generate_cats(cat_files):
    processed = []

    for cat_file in cat_files:
        if STOP:
            raise CancelException

        logging.info(f"Generating cat file: {cat_file}")
        catfilegenerator.generate(cat_file)
        filesigner.sign(cat_file)
        processed.append(cat_file)

    return processed

# Just a syntactic sugar for doing parallel jobs
def do_parallel(fn, args):
    return Parallel(n_jobs=cpu_count())(delayed(fn)(args))

def main(args):
    global STOP
    STOP = False

    try:
        # Start the time!
        start_time = datetime.now()

        # Let's do this
        # Check if main folder exists
        if not os.path.exists(args.main_folder):
            logging.info(f"Source does not exist: {args.main_folder}")
            sys.exit(1)

        # If folder is actually a file then let's do the file scenario
        if os.path.isfile(args.main_folder):
            # Remove that readonly
            COMMON.lprint("Removing the readonly attributes")
            remove_readonly(None, os.path.dirname(args.main_folder), [os.path.basename(args.main_folder)])

            # Sign that signle file
            processed_files = sign_list([args.main_folder])
        else:
            # Remove that readonly
            COMMON.lprint("Removing the readonly attributes")
            os.walk(args.main_folder, remove_readonly, None)

            # Enumerate all the sources and targets
            # Start with the main folder
            # If there are "Included folders", then we will just process those in included folders
            # If none then we will process everything in the main folder
            # This is somewhat hacky, if there are no included folders then we will add the base one
            if len(args.included_folders) <= 0:
                args.included_folders = [""]

            # Let's process the folder in parallel so that we can save time
            processed_files = Parallel(backend="threading", n_jobs=cpu_count(), require="sharedmem")(delayed(process_folder)(os.path.abspath(os.path.join(args.main_folder, included_folder))) for included_folder in args.included_folders)

        # Print the processed files
        COMMON.lprint("List of files that are generated/signed")
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
    except CancelException:
        print("Execution cancelled.")
    except Exception:
        print("Unexpected error:", traceback.format_exc())
        print("\n\nERROR ERROR ERROR")
        sys.exit(1)

def stop():
    global STOP
    STOP = True

class CancelException(Exception):
    pass

if __name__ == "__main__":
    # Parse the arguments
    args = argsparser.parse()
    main(args)