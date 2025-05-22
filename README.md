# Netværksværktøj - Kommandolinje Shell

## Beskrivelse

Et simpelt, brugervenligt kommandolinjeværktøj til netværks- og filhåndtering. Programmet understøtter ping af netværksværter, visning og lagring af resultater, filvisning, og kataloglister – alt sammen fra én shell.

## Funktioner

- **Ping** IP-adresser eller domænenavne med avancerede muligheder:
  - Valgfrit antal ping-forsøg (`-n`)
  - Justerbar pakkestørrelse (`-l`)
  - Justerbar TTL-værdi (`-i`)
  - Justerbar timeout (`-w`)
  - Kontinuerlig ping med `-t` (afslut med Ctrl+C)
  - Gem ping-resultater til fil (`| filnavn`)
- **Vis filindhold** med `cat` (automatisk `.txt`-tilføjelse hvis nødvendigt)
- **List filer og mapper** med `ls` eller `dir` (valgfri sti)
- **Ryd skærmen** med `cls` eller `clear`
- **Hjælp** til alle kommandoer (`?` eller `<kommando> ?`)
- **Afslut programmet** med `exit`, `quit` eller `q`
- **Robust fejl- og inputhåndtering** (f.eks. ugyldige stier, ukendte kommandoer)

## Kommandooversigt

| Kommando                | Beskrivelse                                                                 |
|-------------------------|-----------------------------------------------------------------------------|
| `ping <host> [flags]`   | Send ICMP-echo til vært. Se detaljer nedenfor.                              |
| `cat <filnavn>`         | Vis indholdet af en fil.                                                    |
| `ls [sti]` / `dir [sti]`| List filer og mapper i angivet eller nuværende mappe.                       |
| `cls` / `clear`         | Ryd konsolvinduet.                                                          |
| `?` / `help`            | Vis oversigt over alle kommandoer.                                          |
| `<kommando> ?`          | Vis detaljeret hjælp til en specifik kommando.                              |
| `exit` / `quit` / `q`   | Afslut programmet.                                                          |

### Ping-kommandoen

```
ping <host> [-t] [-n antal] [-i ttl] [-w timeout] [-l størrelse] [| filnavn]
```

**Flag:**
- `-t` : Kontinuerlig ping (afslut med Ctrl+C)
- `-n <antal>` : Antal ping-forsøg (standard: 4)
- `-i <ttl>` : Time To Live-værdi (standard: 128)
- `-w <timeout>` : Timeout i millisekunder (standard: 5000)
- `-l <størrelse>` : Pakkestørrelse i bytes (standard: 32, max: 65500)
- `| <filnavn>` : Gem output til fil (tilføjer `.txt` hvis ikke angivet)

### Eksempler

```sh
ping dr.dk
ping 192.168.1.1 -n 10
ping google.com -l 64 -t
ping 8.8.8.8 -w 1000 -i 64
ping dr.dk | resultater
cat resultater.txt
ls
ls C:\Users\user\Documents
cls
exit
```

### Hjælp

- Skriv `?` for at se alle kommandoer.
- Skriv `<kommando> ?` for detaljeret hjælp til en specifik kommando (f.eks. `ping ?`).

---

Udviklet som en del af "Projekt Ekspert" af studerende ved Campus Bornholm.
