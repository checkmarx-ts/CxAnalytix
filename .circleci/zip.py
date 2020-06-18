import zipfile
import sys
import os
import argparse
import fnmatch

parser = argparse.ArgumentParser(description='Simple zip utility')
parser.add_argument ("--input", help='directory path to compress into the zip', type=str, required=True)
parser.add_argument ("--output", help='file path for the output zip', type=str, required=True)

args = parser.parse_args()

def compress(zip, dir, filterspec, addpath=''):
    for f in os.listdir(dir):
        if os.path.isdir(os.path.join(dir, f)):
            compress(zip, os.path.join(dir, f), filterspec, os.path.join (addpath, f))
        else:
            if (fnmatch.fnmatch(f, filterspec) ):
                zip.write(os.path.join (dir, f), os.path.join (addpath, f))

with zipfile.ZipFile (args.output, mode='w') as zip:
    if os.path.isdir(args.input):
        path = args.input
        filterspec = "*"
    else:
        path = os.path.dirname(args.input)
        filterspec = os.path.basename(args.input)

    compress (zip, path, filterspec)

    zip.close ()

        
