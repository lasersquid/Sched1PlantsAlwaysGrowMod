
param (
    [string]$ver = "il2cpp"
 )

if ($ver -eq "il2cpp") {
    rm -Recurse -Force 'package\il2cpp'
    mkdir 'package\il2cpp\mods'
    Copy 'bin\Debug\net6\PlantsAlwaysGrow.dll' 'package\il2cpp\mods'
    cd 'package\il2cpp'
    Compress-Archive -Path '*' -DestinationPath "..\PlantsAlwaysGrow_IL2CPP.zip"
    cd ..\..
}
elseif ($ver -eq "mono") {
    rm -Recurse -Force 'package\mono'
    mkdir 'package\mono\mods'
    Copy 'bin\Debug\net6\PlantsAlwaysGrowMono.dll' 'package\mono\mods'
    cd 'package\mono'
    Compress-Archive -Path '*' -DestinationPath "..\PlantsAlwaysGrow_Mono.zip"
    cd ..\..
}
else {
    Print 'Specify "-ver il2cpp" or "-ver mono"!'
}