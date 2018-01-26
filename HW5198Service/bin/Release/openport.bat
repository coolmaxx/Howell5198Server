netsh advfirewall firewall Add rule name="Allow port range" dir=out protocol=tcp localport=5198 action=allow
netsh advfirewall firewall Add rule name="Allow port range" dir=in protocol=tcp localport=5198 action=allow
pause