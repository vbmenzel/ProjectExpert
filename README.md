# Netværksværktøj - Ping

## Beskrivelse
Et simpelt kommandolinjeværktøj til at udføre ping-kommandoer mod IP-adresser eller domænenavne. Programmet giver mulighed for at tilpasse antal ping, pakkestørrelse og andre indstillinger.

## Funktioner
- Ping mod IP-adresser eller domænenavne
- Valgfrit antal ping-forsøg (standard: 4)
- Justerbar pakkestørrelse (standard: 32 bytes)
- Justerbar TTL-værdi og timeout
- Kontinuerlig ping med `-t` flag
- Gem/indlæs resultater

## Brug af programmet
```
ping <adresse> [-t] [-n antal] [-i ttl] [-w timeout] [-l størrelse]
cat <fillokation>
```

### Eksempler
```
ping dr.dk
ping 192.168.1.1 -n 10
ping google.com -l 64 -t
ping 8.8.8.8 -w 1000 -i 64
ping dr.drk | C:\Users\user\Documents\save.txt
cat C:\Users\user\Documents\save.txt
```

### Hjælp
Skriv `?` for at se alle kommandoer eller `ping ?` for hjælp til ping.

---

Udviklet som en del af "Projekt Ekspert" af studerende ved Campus Bornholm
