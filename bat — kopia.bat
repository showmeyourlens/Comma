start .\CableCloud\bin\Debug\netcoreapp3.0\CableCloud.exe
ping 192.0.2.1 -n 1 -w 100 >nul
start .\Subnetwork\bin\Debug\Subnetwork.exe "1" "62342"
ping 192.0.2.1 -n 1 -w 100 >nul
start .\ClientNode\bin\Debug\netcoreapp3.0\ClientNode.exe "62331" "C1" "121.32.232.31" "10" "1"
ping 192.0.2.1 -n 1 -w 100 >nul
start .\ClientNode\bin\Debug\netcoreapp3.0\ClientNode.exe "62333" "C2" "171.18.151.27" "12" "1"
ping 192.0.2.1 -n 1 -w 100 >nul
start .\NetworkNode\bin\Debug\netcoreapp3.0\NetworkNode.exe "62337" "OXC1" "167.15.64.182"
ping 192.0.2.1 -n 1 -w 100 >nul
start .\NetworkNode\bin\Debug\netcoreapp3.0\NetworkNode.exe "62340" "OXC2" "196.221.13.245"
ping 192.0.2.1 -n 1 -w 100 >nul
start .\NetworkNode\bin\Debug\netcoreapp3.0\NetworkNode.exe "62341" "OXC4" "191.27.135.99"
ping 192.0.2.1 -n 1 -w 100 >nul