import glob
import os
import kyofilesigner.utilities.common as COMMON

class Compressor:
    """
    Compresses the files
    """
    
    def compress(self, directory: str, recursive: bool = False):
        """
        The compression of course
        """
        # Get the list of all the directory to include
        directory_list = []
        if recursive:
            directory_list = glob.glob(os.path.join(directory, "**", ""), recursive=True)
        else:
            directory_list.append(directory)

        # Run the compressor
        for dirname in directory_list:
            dir_glob = os.path.join(dirname, "*")
            command = f'compress.exe "{dir_glob}" -r'
            cwd = os.path.dirname(os.path.realpath(__file__))
            try:
                COMMON.run_shell_command(command, cwd=cwd)
            except:
                pass
        
        # Delete the uncompressed files
        uncompressed_files = glob.glob(os.path.join(directory, "**", "*[!_]"))
        for uncompressed in uncompressed_files:
            os.remove(uncompressed)

    def compress_single(self, file_location: str, delete_uncompressed: bool):
        """
        Compresses one file
        """
        # Check if it is compressed
        if file_location[-1] == "_":
            return

        command = f'compress.exe "{file_location}" -r'
        cwd = os.path.dirname(os.path.realpath(__file__))
        COMMON.run_shell_command(command, cwd=cwd)

        if delete_uncompressed:
            os.remove(file_location)

        return file_location[:-1] + "_"

    def uncompress_single(self, file_location: str, delete_compressed: bool, target_name: str = ""):
        """
        Uncompresses one file
        """
        # Check if it is uncompressed
        if file_location[-1] != "_":
            return

        if target_name != "":
            command = f'expand "{file_location}" "{target_name}"'
        else:
            command = f'expand "{file_location}" -r'

        COMMON.run_shell_command(command)

        if delete_compressed:
            os.remove(file_location)

        if target_name != "":
            return target_name
        return glob.glob(file_location[:-1] + "?")[0]

if __name__ == "__main__":
    compressor = Compressor()
    compressor.compress(r"C:\Users\Build\Desktop\TEMPFiles\201911-1311-3856-4f7f4ccb-e4a8-4852-b6b1-ebe47982c230 - Copy", recursive=True)