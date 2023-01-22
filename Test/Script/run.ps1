if (!(Test-Path results)) {
	mkdir results > $null
} 

npm install
tsc
node bin/testApi.js


Write-Output "Press enter to continue"
Read-Host
