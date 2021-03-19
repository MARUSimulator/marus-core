function CreateDirectory {
    param (
        $DirectoryPath
    )

    try {
        Write-Host "The directory '$DirectoryPath' does not exist, this needs to be created."
        New-Item -Path $DirectoryPath -ItemType Directory -ErrorAction Stop | Out-Null #-Force
    }
    catch {
        Write-Error -Message "Unable to create directory '$DirectoryPath'. Error was: $_" -ErrorAction Stop 
    }
    Write-Host "Successfully created directory '$DirectoryPath'."

}

###################################################### API Plugins ######################################################

# Creating directories needed for extracting Protobuf and gRPC dependencies.
$CurrentPath = Get-Location
$DownloadPath = Join-Path -Path $CurrentPath -ChildPath "\Temp"
if (-not (Test-Path -LiteralPath $DownloadPath)) {
    CreateDirectory($DownloadPath)
    Write-Host "$DownloadPath will be automatically removed after successful installation."
}

# The path to the file to be unzipped and extracted
$GrpcZipPath = Join-Path -Path $CurrentPath -ChildPath "\Temp\grpc.zip" 

# The path to where the contents of the zip file should be extracted
$APIExtractionDir = Join-Path -Path $CurrentPath -ChildPath "\API\Protobuf\Plugins"
Write-Host "Downloading and setuping protobuf and gRPC... Path: $APIExtractionDir"

# Download the zipped gRPC binaries that are needed. 
Invoke-WebRequest -Uri "https://packages.grpc.io/archive/2019/12/a02d6b9be81cbadb60eed88b3b44498ba27bcba9-edd81ac6-e3d1-461a-a263-2b06ae913c3f/protoc/grpc-protoc_windows_x64-1.26.0-dev.zip" -OutFile ".\Temp\grpc.zip"


# Check whether the Plugins direcotry already exists, if not create it before extracting the zip file.
if (-not (Test-Path -LiteralPath $APIExtractionDir)) {
    CreateDirectory($APIExtractionDir)
    Expand-Archive -LiteralPath $GrpcZipPath -DestinationPath $APIExtractionDir 
}
else {
    Write-Host "The directory '$APIExtractionDir' already exists, stopped extracting contents to prevent overwriting exisiting items."
}



#################################################### UNITY PLUGINS ##################################################

$UnityGrpcPluginsZipPath = Join-Path -Path $CurrentPath -ChildPath "\Temp\unity_grpc.zip"
$UnityPluginsExtractionDir = Join-Path -Path $CurrentPath -ChildPath "\Assets"
$UnityPluginsDir = Join-Path -Path $UnityPluginsExtractionDir -ChildPath "\Plugins"

Write-Host "Downloading and setuping Unity plugins... Path: $UnityPluginsDir"

################################## gRPC #################
# Download the zipped Unity-gRPC plugins that are needed. 

Write-Host "Setup gRPC for Unity"
Invoke-WebRequest -Uri "https://packages.grpc.io/archive/2019/12/a02d6b9be81cbadb60eed88b3b44498ba27bcba9-edd81ac6-e3d1-461a-a263-2b06ae913c3f/csharp/grpc_unity_package.2.26.0-dev.zip" -OutFile ".\Temp\unity_grpc.zip"

# Check if the Unity Plugins directory does not exist.
# If it already exists we do not want to extract the contents
# and overwrite the existing directory
if (-not (Test-Path -LiteralPath $UnityPluginsDir)) {
    Expand-Archive -LiteralPath $UnityGrpcPluginsZipPath -DestinationPath $UnityPluginsExtractionDir
}
else {
    Write-Host "Plugins Directory '$UnityPluginsDir' already exists, extraction stopped to prevent overwriting."  
}


######################## DOWNLOAD CREST FOR UNITY #############
Write-Host "Setup Crest"
$Author = "wave-harmonic"
$Name = "crest" 
$Branch = "master"
$ZipFile = "$DownloadPath\$Name.zip"
$CrestPath = Join-Path -Path $UnityPluginsDir -ChildPath "\Crest"
New-Item $ZipFile -ItemType File -Force
$RepositoryZipUrl = "https://api.github.com/repos/$Author/$Name/zipball/$Branch" 
# download the zip 
Invoke-RestMethod -Uri $RepositoryZipUrl -OutFile $ZipFile
Expand-Archive -Path $ZipFile -DestinationPath $CrestPath -Force

#################################################### CLEANUP ########################################################

# Cleanup, remove Temp download directory since this is not needed anymore
Remove-Item -Recurse $DownloadPath 

Read-Host -Prompt "Setup successful. Press <ENTER> to exit."

