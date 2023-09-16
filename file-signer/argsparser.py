import argparse

def parse():
    parser = argparse.ArgumentParser(prog="KDDP Sign Tool")
    parser.add_argument("-m", "--main-folder", type=str, required=True,
                        help="the main directory path where we will sign the signable files (e.g. '\\10.191.23.75\sd3_release\whatever')")
    parser.add_argument("-f", "--included-folders", type=str, nargs="*",
                        help="add this if you just want to process specific directories (e.g. 'PrnDrv' 'PrnDrv_J')")
    parser.add_argument("-t", "--target", type=str, required=True,
                        help="the directory path where we will save the output (e.g. '\\10.191.23.75\sd3_release\whatever_output')")
    args = parser.parse_args()

    return args