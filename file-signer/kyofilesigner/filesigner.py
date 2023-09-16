"""
Signs the file
"""

import os
import stat
import argparse
import glob
import kyofilesigner.utilities.common as COMMON

class FileSigner:

    def __init__(self, signtool_location="", certificate_location="", certificate_password="#Kyocera1!"):

        # Checking for signtool and certificate
        module_location = os.path.dirname(os.path.realpath(__file__))

        if signtool_location == "":
            signtool_location = os.path.join(module_location, "tools", "signtool", "signtool.exe")

        if certificate_location == "":
            certificate_location = os.path.join(module_location, "tools", "certificate.pfx")

        if not os.path.exists(signtool_location):
            raise Exception("Sign Tool not found")

        if not os.path.exists(certificate_location):
            raise Exception("Certificate not found")

        self.signtool_location = signtool_location
        self.certificate_location = certificate_location
        self.certificate_password = certificate_password

    def sign(self, _file):
        """
        Signs the file
        """
        self.__set_writeable(_file)

        command = f'"{self.signtool_location}" sign /v /f "{self.certificate_location}" /p {self.certificate_password} /tr http://timestamp.digicert.com /td sha256 /fd sha256 "{_file}"'
        try:
            return COMMON.run_shell_command(command, no_print=True) == 0
        except:
            raise Exception(f"Failed to sign {_file}")
    
    def unsign(self, _file):
        """
        Unsigns the file
        """
        self.__set_writeable(_file)
        
        command = f'"{self.signtool_location}" remove /s /v "{_file}"'
        try:
            return COMMON.run_shell_command(command, no_print=True) == 0
        except:
            raise Exception(f"Failed to unsign {_file}")

    def verify(self, _file):
        """
        Verifies if the file is signed
        """
        command = f'"{self.signtool_location}" verify /pa "{_file}"'
        try:
            return COMMON.run_shell_command(command, no_print=True) == 0
        except:
            return False

    def sign_folder(self, folder):
        """
        Signs all the applicable files in the folder
        """
        processed = []
        # Get all the files to sign
        signable_files = self.__retrieve_signable_files(folder)

        # Loop all the signable files
        for _file in signable_files:
            # Check if the file is already signed
            if self.verify(_file):
                self.unsign(_file)

            # Sign it now
            if self.sign(_file):
                processed.append(_file)
        
        return processed

    def unsign_folder(self, folder):
        """
        Unsigns all the applicable files in the folder
        """
        processed = []
        # Get all the files to unsign
        unsignable_files = self.__retrieve_signable_files(folder)

        # Loop all the unsignable files
        for _file in unsignable_files:
            # Check if the file is already unsigned
            if not self.verify(_file):
                self.unsign(_file)

            # Unsign it now
            if self.unsign(_file):
                processed.append(_file)
        
        return processed

    def __retrieve_signable_files(self, folder):
        """
        Retrieves all the signable files in the folder
        """
        pattern_inner_dl_ = os.path.join(folder, "**", "*.dl_")
        pattern_inner_dll = os.path.join(folder, "**", "*.dll")
        pattern_inner_sys = os.path.join(folder, "**", "*.sys")
        pattern_inner_exe = os.path.join(folder, "**", "*.exe")
        pattern_dl_ = os.path.join(folder, "*.dl_")
        pattern_dll = os.path.join(folder, "*.dll")
        pattern_sys = os.path.join(folder, "*.sys")
        pattern_exe = os.path.join(folder, "*.exe")
        files = glob.glob(pattern_dl_) + glob.glob(pattern_dll) + glob.glob(pattern_sys) + glob.glob(pattern_exe) + glob.glob(pattern_inner_dl_, recursive=True) + glob.glob(pattern_inner_dll, recursive=True) + glob.glob(pattern_inner_sys, recursive=True) + glob.glob(pattern_inner_exe, recursive=True)
        # Return unique list
        return list(set(files))

    def __set_writeable(self, _file):
        """
        Removes the read-only attribute of the file
        """
        if os.path.exists(_file):
            os.chmod(_file, stat.S_IWRITE)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(prog="Signer")
    parser.add_argument("-s", "--source", type=str, required=True,
                        help="the directory path where we will sign the signable files (e.g. '\\10.191.23.75\sd3_release\whatever')")
    parser.add_argument("-d", "--destination", type=str, required=True,
                        help="the directory path where we will save the output (e.g. '\\10.191.23.75\sd3_release\whatever_output')")
    # Parse the arguments
    args = parser.parse_args()
    
    signer = FileSigner()
    signer.sign_folder(args.source)