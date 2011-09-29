# -*- coding: iso-8859-15 -*-

# Script to add copyright notices to all ChemProV source files
# Python script tested against v3.1.2

__author__="Adam Carter"
__date__ ="$Jul 22, 2010 1:01:10 PM$"

import os
sourcePath = r'../ChemProV'
validExtensions = ['xaml', 'xaml.cs', 'cs']
specialExtensionChars = {\
                             "xaml" : ['<!--', '-->\n'],\
                             "xaml.cs" : ['/*', '*/\n'],\
                             "cs" : ['/*', '*/\n']
}

copyright = """
Copyright 2010 HELP Lab @ Washington State University

This file is part of ChemProV (http://helplab.org/chemprov).

ChemProV is distributed under the Open Software License ("OSL") v3.0.
Consult "LICENSE.txt" included in this package for the complete OSL license.
"""

for root, dirs, files in os.walk(sourcePath):

    #loop through all files in the directory
    for f in files:

        pieces = f.split(".", 1)
        if len(pieces) < 2:
            continue

        extension = pieces[1]
        if validExtensions.count(extension) > 0:

            #build the file location
            fileLocation = root + '\\' + f
            
            #open file, store old data
            oldFile = open(fileLocation, 'r')
            oldText = oldFile.read()

            #clean out random junk character
            oldText = oldText.replace("﻿", "")
            oldFile.close()

            #replace any existing header comment
            if oldText[0:len(specialExtensionChars[extension][0])] == specialExtensionChars[extension][0]:
                oldText = oldText[oldText.find(specialExtensionChars[extension][1]) + len(specialExtensionChars[extension][1]):]

            #open again, this time prepending the copyright
            newFile = open(fileLocation, 'w')

            #sometimes we need c-style comments, sometimes XML-style comments
            newFile.write(specialExtensionChars[extension][0])

            #write copyright
            newFile.write(copyright)

            #write closing comment tag
            newFile.write(specialExtensionChars[extension][1])

            #write rest of file
            newFile.write(oldText)
            newFile.close()

print("done")