"""Build the real .NET plug-in and Windows Rhino Installer package.
Requires .NET SDK 8; Windows Rhino is needed for host execution validation.
"""
import argparse
from pathlib import Path
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = '2.0.0'
PROJECT = ROOT / 'plugin/BKID.AutoCrop/BKID.AutoCrop.csproj'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--dotnet', default=shutil.which('dotnet'))
    args = parser.parse_args()
    if not args.dotnet:
        parser.error('Install .NET SDK 8 or pass --dotnet /path/to/dotnet')
    subprocess.run([args.dotnet, 'restore', str(PROJECT), '--locked-mode'], check=True)
    subprocess.run([args.dotnet, 'build', str(PROJECT), '-c', 'Release', '--no-restore'], check=True)
    binary = PROJECT.parent / 'bin/Release/net45/BKID.AutoCrop.rhp'
    subprocess.run([args.dotnet, 'run', '--project', str(ROOT / 'tools/VerifyPlugin'),
                    '-c', 'Release', '--', str(binary),
                    str(ROOT / 'BKID_AutoCrop/BKID_AutoCrop.py'),
                    str(ROOT / 'BKID_AutoCrop/assets/icon_64.png')], check=True)
    destination = ROOT / 'installers'
    destination.mkdir(exist_ok=True)
    installer = destination / ('BKID_AutoCrop_' + VERSION + '.rhi')
    with zipfile.ZipFile(installer, 'w', zipfile.ZIP_DEFLATED) as archive:
        info = zipfile.ZipInfo('BKID.AutoCrop.rhp', date_time=(2026, 1, 1, 0, 0, 0))
        info.compress_type = zipfile.ZIP_DEFLATED
        archive.writestr(info, binary.read_bytes())
    with zipfile.ZipFile(installer) as archive:
        if archive.testzip() is not None:
            raise RuntimeError('RHI integrity check failed')
        assert archive.namelist() == ['BKID.AutoCrop.rhp']
        assert archive.read('BKID.AutoCrop.rhp') == binary.read_bytes()
    print('Built and verified', installer.relative_to(ROOT))


if __name__ == '__main__':
    main()
