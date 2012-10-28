################################################################################
## UTILITIES
################################################################################

# Gets the folder this Script is physically located in.
Function Get-Script-Directory
{
    $Scope = 1
    
    while ($True)
    {
        $Invoction = (Get-Variable MyInvocation -Scope $Scope).Value
        
        if ($Invoction.MyCommand.Path -Ne $Null)
        {
            Return Split-Path $Invoction.MyCommand.Path
        }
        
        $Scope = $Scope + 1
    }
}

# Load all required assemblies. When extra assemblies are required,
# add them to this loader method.
Function Load-Assemblies
{
    # Load assemblies required by the build Script.
    
    [void][System.Reflection.Assembly]::LoadFile((Get-Script-Directory) + "\ICSharpCode.SharpZipLib.dll")
}

# Compress a folder into a ZIP file. $Filter is interpreted as a
# a regular expression against which every file is matched.
Function Compress([string]$Target, [string]$Path, [bool]$Recursive = $True, [string]$Filter = $Null)
{
    (New-Object ICSharpCode.SharpZipLib.Zip.FastZip).CreateZip($Target, $Path, $Recursive, $Filter)
}

################################################################################
## DIRECTORY STRUCTURE
################################################################################

# Prepare the distribution folder.
Function Prepare-Directories
{
    Prepare-Directory $Global:Paths.Distrib
    Prepare-Directory $Global:Paths.Release
}

# Prepare a distribution folder.
Function Prepare-Directory([string]$Path)
{
    if (Test-Path -Path $Path)
    {
        Remove-Item -Recurse -Force $Path
    }

    [void](New-Item -Type directory $Path)
}

################################################################################
## COMPRESS
################################################################################

# Compress the distrib folder.
Function Compress-Distrib
{
    Write-Host "Creating ZIP file"
    
    $TargetPath = $Global:Paths.Release + "\ProtoChannel.zip"
    
    Compress -Target $TargetPath -Path $Global:Paths.Distrib
}

################################################################################
## GATHER FILES
################################################################################

# Gather all files that will be part of the distrib.
Function Gather-Files
{
    Write-Host "Gathering files"
    
    Copy-Item ($Global:Paths.Root + "License.txt") -Destination ($Global:Paths.Distrib + "License.txt")
    
    Copy-Release -Project "ProtoChannel" -File "ProtoChannel-2.dll" -Type "ProtoChannel"
    Copy-Release -Project "ProtoChannel" -File "ProtoChannel-4.dll" -Type "ProtoChannel"
    Copy-Release -Project "ProtoChannel" -File "ProtoChannel-MD.dll" -Type "ProtoChannel"
    Copy-Release -Project "ProtoChannel" -File "protobuf-net.dll" -Type "ProtoChannel"
    Copy-Release -Project "ProtoChannel" -File "Common.Logging.dll" -Type "ProtoChannel"
    Copy-Release -Project "ProtoChannel.CodeGenerator" -File "codegen.exe" -Type "Tools"
    Copy-Release -Project "ProtoChannel.HttpProxy" -File "prototype.js" -Type "Web Dependencies"
    Copy-Release -Project "ProtoChannel.HttpProxy" -File "protochannel.js" -Type "Web Dependencies"
    Copy-Release -Project "ProtoChannel.HttpProxy" -File "Web.config" -Type "Web Dependencies"
    Copy-Release -Project "ProtoChannel.Web" -File "Common.Logging.dll" -Type "ProtoChannel Web"
    Copy-Release -Project "ProtoChannel.Web" -File "Newtonsoft.Json.dll" -Type "ProtoChannel Web"
    Copy-Release -Project "ProtoChannel.Web" -File "protobuf-net.dll" -Type "ProtoChannel Web"
    Copy-Release -Project "ProtoChannel.Web" -File "ProtoChannel.Web.dll" -Type "ProtoChannel Web"
    Copy-Release -Project "ProtoChannel.Web" -File "ProtoChannel-4.dll" -Type "ProtoChannel Web"
}

# Copy a single release file.
Function Copy-Release($Project, $File, $Type)
{
    $Path = $Global:Paths.Root + $Project + "\" + $File
    $Target = $Global:Paths.Distrib + $Type + "\"
    
    if ((Test-Path $Target) -eq $False)
    {
        [void](New-Item -Type directory $Target)
    }
    
    $Target = $Target + $File
    
    if ((Test-Path $Path) -eq $False)
    {
        $Path = $Global:Paths.Root + $Project + "\bin\Release\" + $File
    }
    
    Copy-Item $Path -Destination $Target
}

################################################################################
## MAIN
################################################################################

$Global:Paths = @{ }

$Global:Paths.Root = (Get-Item (Get-Script-Directory)).Parent.Parent.FullName + "\"
$Global:Paths.Build = $Global:Paths.Root + "Build\"
$Global:Paths.Distrib = $Global:Paths.Build + "Distrib\"
$Global:Paths.Release = $Global:Paths.Build + "Release\"

Load-Assemblies

Prepare-Directories

Gather-Files

Compress-Distrib
