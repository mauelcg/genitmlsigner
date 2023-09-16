from setuptools import setup
from version import __version__

setup(name="KyoFileSigner",
      version=__version__,
      description="Signs the file with the kyocera certificate",
      url="//depot/Projects/CDROM/CD Automation/KDDP CD Team OTP/file-signer/",
      author="KDDP SD33",
      license="MIT",
      packages=[
            "kyofilesigner",
            "kyofilesigner.utilities",
            "kyofilesigner.compressor",
      ],
      package_data={
            "kyofilesigner": ["tools/*", "tools/inf2cat/*",  "tools/signtool/*"]
      },
      install_requires=[
          "pywin32",
          "p4wrapper",
          "joblib",
          "pysimplegui"
      ],
      zip_safe=False,
      )