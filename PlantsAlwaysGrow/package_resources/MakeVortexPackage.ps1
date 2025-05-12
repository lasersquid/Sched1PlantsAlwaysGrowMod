
param (
    [string]$ver = "il2cpp"
 )

  # Check param
 if ("$ver" -eq "il2cpp") {
    $dll_file = "PlantsAlwaysGrow.dll"
    $arch_str = "IL2CPP"
}
elseif ("$ver" -eq "mono") {
    $dll_file = "PlantsAlwaysGrowMono.dll"
    $arch_str = "Mono"
}
else {
    Write-Output 'Specify "-ver il2cpp" or "-ver mono"!'
    Exit -1
}

rm -Recurse -Force "package\$($ver)"
mkdir "package\vortex\$($ver)\mods"
Copy "bin\Debug\net6\$($dll_file)" "package\vortex\$($ver)\mods"
cd "package\vortex\$($ver)"
Compress-Archive -Path '*' -DestinationPath "..\PlantsAlwaysGrow_$($arch_str).zip"
cd ..\..\..