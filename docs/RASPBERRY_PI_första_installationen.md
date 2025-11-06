# Installera entity framework på Raspberry pi: 

```terminal
username@raspberrypi:~/Privatekonomi $ dotnet tool install --global dotnet-ef
Tools directory '/home/username/.dotnet/tools' is not currently on the PATH environment variable.
If you are using bash, you can add it to your profile by running the following command:

cat << \EOF >> ~/.bash_profile
# Add .NET Core SDK tools
export PATH="$PATH:/home/username/.dotnet/tools"
EOF

You can add it to the current session by running the following command:

export PATH="$PATH:/home/username/.dotnet/tools"

You can invoke the tool using the following command: dotnet-ef
Tool 'dotnet-ef' (version '9.0.10') was successfully installed.
username@raspberrypi:~/Privatekonomi $ cat << \EOF >> ~/.bash_profile
# Add .NET Core SDK tools
export PATH="$PATH:/home/username/.dotnet/tools"
EOF
username@raspberrypi:~/Privatekonomi $ export PATH="$PATH:/home/username/.dotnet/tools"


username@raspberrypi:~/Privatekonomi $ dotnet tool install --global dotnet-ef
Tool 'dotnet-ef' is already installed.
```

# Installera dev-cert
`dotnet dev-certs https --clean`

`dotnet dev-certs https --trust`

# Starta applikationen
```
cd "src/Privatekonomi.AppHost"
dotnet run
```



# Nå webbsidan via andra enheter på nätverket

Fråga: 
Hur öppnar jag porten: 17127 från min raspberry pi till andra enheter på nätverket för min dotnet applikation? 

--------- 

Kort svar: På Raspberry Pi behöver du normalt inte “öppna” portar lokalt. Det viktiga är att din .NET-app lyssnar på rätt nätverksinterface (inte bara localhost) och att ingen brandvägg blockerar. Gör så här:

1) Kontrollera att appen lyssnar på 0.0.0.0:17127
- Starta appen temporärt så att den lyssnar på alla nätverkskort:
  - ASP.NET Core:
    - Engångskörning:
      ```
      ASPNETCORE_URLS=http://0.0.0.0:17127 dotnet MyApp.dll
      ```
      eller
      ```
      dotnet run --urls "http://0.0.0.0:17127"
      ```
    - I kod (Program.cs):
      ```csharp
      builder.WebHost.UseUrls("http://0.0.0.0:17127");
      ```
    - Via konfig (appsettings.json):
      ```json
      {
        "Kestrel": {
          "Endpoints": {
            "Http": {
              "Url": "http://0.0.0.0:17127"
            }
          }
        }
      }
      ```
  - Egen TCP/UDP-server: bind till 0.0.0.0 (eller Pi:ns LAN-IP) på port 17127.
- Verifiera att porten lyssnar:
  ```
  ss -lntp | grep 17127    # TCP
  ss -lnup | grep 17127    # UDP (om relevant)
  ```

2) Brandvägg (ufw) – om aktiverad
- Raspberry Pi OS har oftast ingen aktiv brandvägg, men om du använder ufw:
  ```
  sudo apt-get update
  sudo apt-get install -y ufw
  sudo ufw status
  sudo ufw allow 17127/tcp
  # (och/eller) sudo ufw allow 17127/udp
  sudo ufw reload
  sudo ufw status
  ```

3) Testa från annan enhet på samma nät
- Byt 192.168.x.y mot Pi:ns IP:
  ```
  curl http://192.168.x.y:17127/
  # eller
  nc -vz 192.168.x.y 17127
  ```
- Om det inte fungerar: kontrollera att appen inte bara lyssnar på 127.0.0.1:
  ```
  ss -lntp | grep 17127
  ```
  Ska visa 0.0.0.0:17127 eller [::]:17127 (inte 127.0.0.1:17127).

4) Köra som systemd-tjänst (rekommenderat)
- Sätt URL via environment så den alltid lyssnar på nätet.

```ini name=/etc/systemd/system/myapp.service
[Unit]
Description=My .NET app
After=network-online.target
Wants=network-online.target

[Service]
WorkingDirectory=/opt/myapp
ExecStart=/usr/bin/dotnet /opt/myapp/MyApp.dll
# Lyssna på alla interface på port 17127
Environment=ASPNETCORE_URLS=http://0.0.0.0:17127
# (lägg till annan miljökonfig här vid behov)
Restart=always
RestartSec=5
User=pi
# Säkrare filbehörigheter
UMask=0027

[Install]
WantedBy=multi-user.target
```

- Aktivera och starta:
  ```
  sudo systemctl daemon-reload
  sudo systemctl enable --now myapp.service
  sudo systemctl status myapp.service
  ```

5) Vanliga orsaker när det ändå inte fungerar
- Appen lyssnar bara på localhost (fixa med UseUrls/ASPNETCORE_URLS).
- Fel protokoll (TCP vs UDP) – öppna rätt protokoll i ufw och bind rätt i appen.
- Docker/kontainer: glöm inte port-publicering:
  ```
  docker run -p 17127:17127 <image>
  ```
- Fel IP används vid test – kontrollera Pi:ns IP:
  ```
  hostname -I
  ```

Behöver du hjälp att sätta detta för just din app (t.ex. Program.cs eller appsettings), klistra in hur du startar appen idag så visar jag exakt ändringen.




