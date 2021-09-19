# WISRAT
Windows Is Shit Remote Administration Tool

## How to use
1) Create files `banlist.txt` and `ipbanlist.txt`
2) Start a server: `wisrat server <ip> <port> <password>`
3) Patch a client: `wisrat client <filename> <username> <password> fetch <url>`
   OR `wisrat client <filename> <username> <password> direct <ip> <port>`
4) Start the client, done!

## Destructive mode
**This is not already done!**
It can be enabled if you add `wisrat client ... destructive` argument.
### Features
1) Autorun (Windows only)
2) Bluescreen on kill (Windows only)
3) Ignore SIGINT
