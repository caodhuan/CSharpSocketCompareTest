import os

command = "protoc --csharp_out=. command.proto"
os.system(command)

print("done")
raw_input()