import maya.cmds as mc
import os

def texStatus():
    #quick check stats for all the texture files in the scene.

    texDirs = []
    fileList = mc.ls(type = "file")
    
    #prints out all files and their absolute path.
    print "\n========= All File Path ==============="
    print "Found", str(len(fileList)), "files in the scene:\n"
    
    for f in fileList:
        fPath = mc.getAttr(f+".fileTextureName")
        
        dirPath = os.path.split(fPath)[0]
        
        print f + ":"
        print fPath + "\n"
        print "test: " + "..\..\..\Textures\\" + fPath + "\n"
        mc.setAttr(f+".fileTextureName", "..\..\..\Textures\\" + fPath, type = "string")

        if dirPath not in texDirs:
            texDirs.append(dirPath)

    #prints out all folders being used for the textures
    #print "\n========= All FOLDERS USED ==============="
    #for p in texDirs:
    #    print p

    #prints out the folder is doesnt's exist
    #print "\n========= File Path not Exist ==============="
    #for p in texDirs:
    #    try:
    #        if not os.path.exists(p):
    #            print p
    #    except e:
    #        print error

    #prints out the folder if it's empty
    #print "\n========= File Path Empty ==============="
    
    #for p in texDirs:
    #    try:
    #        if len(os.listdir(p)) == 0:
    #            print p
    #    except e:
    #        print error

#run
texStatus()