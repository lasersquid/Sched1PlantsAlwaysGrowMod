
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
    Write-Output 'Specify "-arch il2cpp" or "-ver mono"!'
    Exit -1
}

if ("$($proj)" -eq "") {
    Write-Output 'Specify "-proj <projectname>"!'
    Exit -1
}

$zip_file = "$($proj)_$($arch_str)-$($ver).zip"

# Clean and create directory structure
rm -Recurse -Force "package\vortex\$($arch)"
rm -Force "package\vortex\$($zip_file)"
mkdir "package\vortex\$($arch)\mods"

# Copy the files
Write-Output "copy from path: bin\Debug\$($net_ver)\$($dll_file)"
Copy "bin\Debug\$($net_ver)\$($dll_file)" "package\vortex\$($arch)\mods"

# Zip it all up
cd "package\vortex\$($arch)"
Compress-Archive -Path '*' -DestinationPath "..\$($zip_file)"

cd ..\..\..