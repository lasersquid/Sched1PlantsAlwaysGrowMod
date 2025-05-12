
param (
    [string]$ver = "1.0.0",
    [string]$arch = "il2cpp"
 )

 # Check param
 if ("$arch" -eq "il2cpp") {
    $dll_file = "PlantsAlwaysGrow.dll"
    $arch_str = "IL2CPP"
}
elseif ("$arch" -eq "mono") {
    $dll_file = "PlantsAlwaysGrowMono.dll"
    $arch_str = "Mono"
}
else {
    Write-Output 'Specify "-arch il2cpp" or "-arch mono"!'
    Exit -1
}

# Clean and create directory structure
rm -Recurse -Force "package\thunderstore\$($arch)" 
rm -Force "package\thunderstore\PlantsAlwaysGrow_$($arch_str).zip"
mkdir "package\thunderstore\$($arch)\Mods"

# Copy the files
Copy "bin\Debug\net6\$($dll_file)" "package\thunderstore\$($arch)\Mods"
Copy 'package_resources\cannabis_leaf.png' "package\thunderstore\$($arch)\icon.png"
Copy 'package_resources\README.md' "package\thunderstore\$($arch)\README.md"
Copy 'package_resources\manifest.json' "package\thunderstore\$($arch)\manifest.json"

# Set version and arch strings
$json = [System.IO.File]::ReadAllText("package\thunderstore\$($arch)\manifest.json")
$json = $json.Replace('%%VERSION%%', $ver)
$json = $json.Replace('%%ARCH%%', $arch_str)
[System.IO.File]::WriteAllText("package\thunderstore\$($arch)\manifest.json", $json)

# Zip it all up
cd "package\thunderstore\$($arch)"
Compress-Archive -Path '*' -DestinationPath "..\PlantsAlwaysGrow_$($arch_str).zip"
cd ..\..\..