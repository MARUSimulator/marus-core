#!/bin/bash
createDirectory() {
    if [ ! -d $1 ]; then
        mkdir -p $1
    fi
}

CurrentPath=$PWD

if [[ $CurrentPath == *setup ]] 
then
    CurrentPath="$CurrentPath/.."
fi

DownloadPath="$CurrentPath/Temp";
if [ ! -d $DownloadPath ]; then 
    createDirectory $DownloadPath;
    echo "$DownloadPath will be automatically removed after successful installation.";
fi

# The path to the file to be unzipped and extracted
GrpcZipPath="$CurrentPath/Temp/grpc.zip";

# The path to where the contents of the zip file should be extracted
APIExtractionDir="$CurrentPath/API/Protobuf/Plugins";
echo "Downloading and setuping protobuf and gRPC... Path: $APIExtractionDir";


# Check whether the Plugins direcotry already exists, if not create it before extracting the zip file.
if [ ! -d $APIExtractionDir ]; then
    createDirectory $APIExtractionDir;
fi
if [ !"$(ls -A $APIExtractionDir)" ]; then
	curl "https://packages.grpc.io/archive/2021/04/ccbb2dd207635cd1c53d9255a4b82c96e6042b74-b1949379-fd83-4d31-abf8-822fae110b43/protoc/grpc-protoc_linux_x64-1.38.0-dev.tar.gz" -o $GrpcZipPath;
	tar -zxvf $GrpcZipPath -C $APIExtractionDir;
else
	echo "The directory '$APIExtractionDir' already exists, stopped extracting contents to prevent overwriting exisiting items.";
fi

#################################################### UNITY PLUGINS ##################################################

UnityGrpcPluginsZipPath="$CurrentPath/Temp/unity_grpc.zip";
UnityPluginsExtractionDir="$CurrentPath/Assets";
UnityPluginsDir="$UnityPluginsExtractionDir/Plugins";

# Check if the Unity Plugins directory does not exist.
# If it already exists we do not want to extract the contents
# and overwrite the existing directory
if [ ! -d $UnityPluginsDir ]; then
    createDirectory $UnityPluginsDir;
fi

if [ !"$(ls -A $UnityPluginsDir)" ]; then
	echo "Downloading and setuping Unity plugins... Path: $UnityPluginsDir";
	curl "https://packages.grpc.io/archive/2021/04/ccbb2dd207635cd1c53d9255a4b82c96e6042b74-b1949379-fd83-4d31-abf8-822fae110b43/csharp/grpc_unity_package.2.38.0-dev202104010955.zip" -o $UnityGrpcPluginsZipPath;
	unzip $UnityGrpcPluginsZipPath -d $UnityPluginsDir;
else
	echo "The directory '$APIExtractionDir' already exists, stopped extracting contents to prevent overwriting exisiting items.";
fi

# Cleanup, remove Temp download directory since this is not needed anymore
rm -rf $DownloadPath ;

echo "Setup successful. Press <ENTER> to exit.";



