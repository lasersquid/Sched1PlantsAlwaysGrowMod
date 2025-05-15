
param (
    [string]$ver = "1.0.0",
    [string]$arch = "il2cpp",
    [string]$proj = ""
 )

 # Check params
 if ("$arch" -eq "il2cpp") {
    $dll_file = "$($proj).dll"
    $arch_str = "IL2CPP"
    $net_ver = "net6"
}
elseif ("$arch" -eq "mono") {
    $dll_file = "$($proj)Mono.dll"
    $arch_str = "Mono"
    $net_ver = "netstandard2.1"
}
else {
    Write-Output 'Specify "-arch il2cpp" or "-arch mono"!'
    Exit -1
}

if ("$proj" -eq "") {
    Write-Output 'Specify "-proj <projectname>""!'
    Exit -1
}

$zip_file = "$($proj)_$($arch_str)-$($ver).zip"

# Clean and create directory structure
rm -Recurse -Force "package\thunderstore\$($arch)" 
rm -Force "package\thunderstore\$($zip_file)"
mkdir "package\thunderstore\$($arch)\Mods"

# Copy the files
Copy "bin\Debug\$($net_ver)\$($dll_file)" "package\thunderstore\$($arch)\Mods"
Copy 'package_resources\icon.png' "package\thunderstore\$($arch)\icon.png"
Copy 'package_resources\README.md' "package\thunderstore\$($arch)\README.md"
Copy 'package_resources\manifest.json' "package\thunderstore\$($arch)\manifest.json"

# Set version and arch strings
$json = [System.IO.File]::ReadAllText("package\thunderstore\$($arch)\manifest.json")
$json = $json.Replace('%%VERSION%%', $ver)
$json = $json.Replace('%%ARCH%%', $arch_str)
[System.IO.File]::WriteAllText("package\thunderstore\$($arch)\manifest.json", $json)

# Zip it all up
cd "package\thunderstore\$($arch)"
Compress-Archive -Path '*' -DestinationPath "..\$($zip_file)"

cd ..\..\..