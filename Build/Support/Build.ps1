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

################################################################################
## BUILD NUGET PACKAGES
################################################################################

Function Get-Version
{
    $Content = Get-Content ($Global:Paths.Root + "ProtoChannel\Properties\AssemblyInfo.cs")
    
    $Matches = ([regex]"AssemblyVersion\(`"(.*?)`"\)").Match($Content)
    
    return $Matches.Groups[1].Value
}

Function Build-NuGet-Packages
{
    Write-Host "Building NuGet packages"

    $CurrentDirectory = [Environment]::CurrentDirectory
    [Environment]::CurrentDirectory = $Global:Paths.Build
    
    $Version = Get-Version
    
    & ($Global:Paths.NuGet) pack ProtoChannel.nuspec -prop ("version=" + $Version) -NoPackageAnalysis
    & ($Global:Paths.NuGet) pack ProtoChannel.Web.nuspec -prop ("version=" + $Version)
    
    [Environment]::CurrentDirectory = $CurrentDirectory
}

################################################################################
## MAIN
################################################################################

$Global:Paths = @{ }

$Global:Paths.Root = (Get-Item (Get-Script-Directory)).Parent.Parent.FullName + "\"
$Global:Paths.Build = $Global:Paths.Root + "Build\"
$Global:Paths.NuGet = $Global:Paths.Root + "Libraries\NuGet\NuGet.exe"

Build-NuGet-Packages
