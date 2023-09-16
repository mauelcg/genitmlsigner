"""
Creates the CAT file in the specified directory
"""

import sys
import os
import stat
import glob
import logging
import re
import kyofilesigner.utilities.common as COMMON

class CatFileGenerator:

    OS_LIST = {
        "32bit": {
            "win81above": "8_X86,7_X86,Server2008_x86,6_3_X86,10_X86,10_AU_X86,10_RS2_X86,10_RS3_X86,10_RS4_X86,10_RS5_X86,10_19H1_X86,10_VB_X86"
        },
        "64bit": {
            "win81above": "8_X64,7_X64,6_3_X64,Server2008R2_X64,Server2008_X64,Server8_X64,Server6_3_X64,10_X64,SERVER2016_X64,10_AU_X64,Server10_X64,10_RS2_X64,10_RS3_X64,10_RS4_X64,ServerRS5_X64,10_RS5_X64,10_19H1_X64,10_VB_X64,ServerFE_X64,10_CO_X64,10_NI_X64"
        },
        "arm64": {
            "win81above": "Server10_ARM64,10_RS3_ARM64,10_RS4_ARM64,ServerRS5_ARM64,10_RS5_ARM64,10_19H1_ARM64,10_VB_ARM64,ServerFE_ARM64,10_CO_ARM64,10_NI_ARM64"
        }
    }

    def __init__(self, inf2cat_location=r"C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\Inf2Cat.exe"):
        self.inf2cat_location = inf2cat_location

    def generate(self, catname: str, os_names="win81above"):
        """
        Generates CAT file under the specified directory
        """
        try:
            logging.info(f"Generating CAT")
            catdir = os.path.dirname(catname)
            # Get the inf file
            pattern = os.path.join(catdir, "*.inf")
            infs = glob.glob(pattern)
            logging.info(f"Generating CAT: infs : {infs}")
            if len(infs) <= 0:
                return False
            inf_file = infs[0]

            # Delete the previous CAT
            logging.info(f"Generating CAT: Delete previous")
            if os.path.exists(catname):
                # Remove read only
                self.__set_writeable(catname)
                os.remove(catname)

            # Call the INF2CAT
            is_64bit = self.__is_64bit(inf_file)
            is_arm64 = self.__is_arm64(inf_file)
            self.__create_cat(catdir, is_64bit, is_arm64, os_names)
            return True
        except Exception as e:
            raise Exception(f"Error generating CAT file {catname}: {e}")

    def __is_64bit(self, oemsetup_file: str):
        """
        Get's the cat file name
        """
        contents = open(oemsetup_file, "r").read()
        regex = r'^"?(Kyocera|%Mfg%|TA/UTAX|UTAX/TA|RISO|LANXUM|%ManufacturerName%|Olivetti|Fujitsu|FujitsuFCCL)"?\s*=\s*\"?(.+?)\"?$'
        output = re.search(regex, contents, re.MULTILINE | re.IGNORECASE)
        manufacturers = output.groups()[-1].split(",")

        # Check if there exist an amd64 or ia64
        for manufacturer in manufacturers:
            if "amd64" in manufacturer or "ia64" in manufacturer:
                return True
        return False

    def __is_arm64(self, oemsetup_file: str):
        """
        Get's the cat file name
        """
        contents = open(oemsetup_file, "r").read()
        regex = r'^"?(Kyocera|%Mfg%|TA/UTAX|UTAX/TA|RISO|LANXUM|%ManufacturerName%|Olivetti|Fujitsu|FujitsuFCCL)"?\s*=\s*\"?(.+?)\"?$'
        output = re.search(regex, contents, re.MULTILINE | re.IGNORECASE)
        manufacturers = output.groups()[-1].split(",")

        # Check if there exist an arm64
        for manufacturer in manufacturers:
            if "arm64" in manufacturer:
                return True
        return False

    def __create_cat(self, directory: str, is_64bit: bool, is_arm64: bool, os_names: list):
        """
        Create's the CAT file
        """
        logging.info(f"Generating CAT file for {directory}")
        
        if is_arm64:
            os_field = self.OS_LIST["arm64"][os_names]
        elif is_64bit:
            os_field = self.OS_LIST["64bit"][os_names]
        else:
            os_field = self.OS_LIST["32bit"][os_names]

        command = [f'"{self.inf2cat_location}"', f'/driver:"{directory}"', f'/os:{os_field}', "/uselocaltime"]
        COMMON.run_shell_command(" ".join(command))

    def __set_writeable(self, _file):
        """
        Removes the read-only attribute of the file
        """
        os.chmod(_file, stat.S_IWRITE)

if __name__ == "__main__":
    generator = CatFileGenerator(r"C:\Program Files (x86)\Windows Kits\10\bin\10.0.17763.0\x86\Inf2Cat.exe")
    cats = generator.generate(r"\\10.191.23.50\sd31_shared\KXDriver\For Cio\ND04-3059\KXDriver v8.0.1329_signed\64bit\utax.cat")