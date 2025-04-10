import React, { useState } from "react";
import ReactMarkdown from "react-markdown";
import style from "./Documentation.module.css";

const sections = [
  { id: "home", label: "Főoldal" },
  { id: "auth", label: "Autentikáció" },
  { id: "rooms", label: "Szállások" },
  { id: "reserve", label: "Szállás foglalása" },
  { id: "reservations", label: "Foglalások" },
  { id: "profile", label: "Profil" },
  { id: "contact", label: "Kapcsolatfelvétel" },
  { id: "support", label: "Technikai támogatás" },
  { id: "faq", label: "GYIK (Gyakori kérdések)" },
  { id: "terms", label: "Felhasználási feltételek" },
  { id: "privacy", label: "Adatvédelem" },
  { id: "devdocs", label: "Fejlesztői Dokumentáció" },
];

const markdownContent = {
  home: `
  ## 🏠 Home (Főoldal)
  
  ### 📋 Leírás:
  A CozyNest főoldala a nyitóképernyő, amely bemutatja a szolgáltatás lényegét, kiemeli annak előnyeit, és bizalmat épít a látogatókban. A letisztult design és a gördülékeny animációk célja, hogy barátságos és modern élményt nyújtson már az első pillanattól.
  
  ### 🗂️ Szekciók áttekintése:
  
  #### 1. Bemutatkozás
  - **Logó:** COZYNEST
  - **Szlogen:** „Egy kattintás a nyugodt pihenéshez.”
  - A márka bemutatása, bizalomépítés, első benyomás.
  
  #### 2. Rólunk szekció
  - Rövid szöveges bemutató arról, hogyan segít a CozyNest szállást találni.
  - Kiemelések:
    - Gyors, egyszerű és biztonságos foglalás
    - Intuitív kezelőfelület
  
  #### 3. Előnyök és vélemények
  - **Szolgáltatás előnyei:**
    - 🌍 Széles szálláskínálat
    - 🔒 Biztonságos adatkezelés
  - **Felhasználói vélemények:**
    - 3 vendég visszajelzése (név, város, értékelés)
    - Segít a döntésben, hitelességet ad
  
  #### 4. Kapcsolat
  - Kapcsolatfelvételi űrlap (\`Contact\` szekció)
  - A látogatók közvetlenül üzenhetnek kérdés vagy érdeklődés esetén
  
  ### ✅ Funkciók:
  - Átlátható szerkezet, szakaszokra bontva
  - Scroll indikátor mutatja a felhasználónak, hol tart az oldalon
  - Animált elemek a tartalom gördülékeny megjelenítéséhez
  - Reszponzív kialakítás mobilra és tabletre is
  
  ### 🧭 Navigáció:
  - Az oldal szakaszai belső hivatkozásokkal érhetők el:
    - \`#story\` – Rólunk
    - \`#info\` – Előnyök és vélemények
    - \`#contact\` – Kapcsolat
  - Sikeres űrlapküldés után visszajelzés jelenik meg
  
  ### ⚠️ Lehetséges hibák:
  
  #### 1. Adatok betöltési hiba  
  **Hibaüzenet:** „Az adatok betöltése sikertelen. Kérjük, frissítse az oldalt.”  
  ![Hiba képernyőképe](/loginError.png)
  
  #### 2. Kapcsolatfelvétel sikertelen  
  **Hibaüzenet:** „Hiba történt az űrlap elküldésekor.”  
  ![Hiba képernyőképe](/loginError.png)
  

.
---
.
.
---
  `,
  auth: `
## 🔐 Auth (Bejelentkezés / Regisztráció)

### 📋 Leírás:
A bejelentkezés és regisztráció képernyő lehetővé teszi a felhasználók számára, hogy hozzáférjenek CozyNest fiókjukhoz, vagy új fiókot hozzanak létre.

## ✅ Funkciók
- Bejelentkezés e-mail címmel és jelszóval.
- Regisztráció új fiók létrehozásához.
- Jelszó mezőben biztonsági ellenőrzések (min. 8 karakter, nagybetű, szám stb.).
- „Elfelejtett jelszó” funkció.

## 📑 Regisztrációs követelmények

#### ✅ Felhasználónév követelmények
- A mező kitöltése kötelező
- Nem kezdődhet számmal vagy speciális karakterrel
- Csak betűket (\`a–z\`, \`A–Z\`), számokat (\`0–9\`) és aláhúzásjeleket (\`_\`) tartalmazhat
- Legalább **3 karakter** hosszú legyen
- Legfeljebb **20 karakter** hosszú lehet

#### ✅ Jelszó követelmények
- A mező kitöltése kötelező
- Legalább **8 karakter** hosszú legyen
- Tartalmazzon legalább:
  - **1 nagybetűt** (pl. \`A\`)
  - **1 kisbetűt** (pl. \`a\`)
  - **1 számot** (pl. \`1\`)
  - **1 speciális karaktert** (pl. \`!@#$%^&*\`)
- **Ne tartalmazzon szóközt**
- **Ne tartalmazza a felhasználónevet vagy az e-mail címet**

#### ✅ E-mail követelmények
- A mező kitöltése kötelező
- Érvényes e-mail formátum szükséges (pl. \`nev@domain.hu\`)

### 🧭 Navigáció:
- Automatikus átirányítás a főoldalra sikeres belépés után.
- Hiba esetén a mezők alatt hibaüzenet jelenik meg.

### ⚠️ Lehetséges hibák:

#### 1. Hibás bejelentkezési adatok  
**Hibaüzenet:** „Hibás e-mail vagy jelszó.”  
![Hiba képernyőképe](/LoginErrorPassword.png)

#### 2. Már regisztrált e-mail  
**Hibaüzenet:** „Ez az e-mail cím már használatban van.”  
![Hiba képernyőképe](/RegEmailUsedError.png)


#### 3. Hiányzó mezők  
**Hibaüzenet:** „Kérjük, töltse ki az összes mezőt.”  
![Hiba képernyőképe](/loginError.png)

#### 4. Érvénytelen e-mail formátum  
**Hibaüzenet:** „Érvénytelen e-mail cím.”  
![Hiba képernyőképe](/RegInvalidEmail.png)

#### 5. Érvénytelen felhasználónév  
**Hibaüzenetek:**
- „A felhasználónév megadása kötelező.”
- „A felhasználónév nem kezdődhet számmal vagy speciális karakterrel.”
- „A felhasználónév csak betűket, számokat és aláhúzásjeleket tartalmazhat.”
- „A felhasználónév túl rövid. Minimum 3 karakter szükséges.”
- „A felhasználónév túl hosszú. Maximum 20 karakter engedélyezett.”  
<div className="image-container">📷 *(Ide jön a hiba képernyőképe)*</div>

#### 6. Érvénytelen jelszó  
**Hibaüzenetek:**
- „A jelszó megadása kötelező.”
- „A jelszónak legalább 8 karakter hosszúnak kell lennie.”
- „A jelszónak tartalmaznia kell legalább egy nagybetűt.”
- „A jelszónak tartalmaznia kell legalább egy kisbetűt.”
- „A jelszónak tartalmaznia kell legalább egy számot.”
- „A jelszónak tartalmaznia kell legalább egy speciális karaktert (!@#$%^&*).”
- „A jelszó nem tartalmazhat szóközt.”
- „A jelszó nem tartalmazhatja a felhasználónevet vagy az e-mail címet.”  
<div className="image-container">📷 *(Ide jön a hiba képernyőképe)*</div>

#### 7. Jelszavak nem egyeznek  
**Hibaüzenet:** „A megadott jelszavak nem egyeznek.”  

![Hiba képernyőképe](/pwNoMatch.png)
--

#### 8. Érvénytelen vagy hiányzó e-mail  
**Hibaüzenetek:**
- „Az e-mail cím megadása kötelező.”
- „Érvénytelen e-mail cím formátum.”  

![Hiba képernyőképe](/RegInvalidEmail.png)
#### 9. Ismeretlen hiba / szerverhiba  
**Hibaüzenet:** „Ismeretlen hiba történt. Kérjük, próbálja újra később.”  (Nem fut a backend / Nem elérhető)

![Hiba képernyőképe](/failFetch.png)

.
.
---
`,
  rooms: `
## 🛏️ Rooms (Szobák)

### 📋 Leírás:
A "Szobák" oldal lehetővé teszi a felhasználók számára, hogy elérhető szállásokat böngésszenek, szűrjenek és foglaljanak a kívánt dátumokra a CozyNest rendszeren belül.


## ✅ Funkciók

- Szobák listázása a kiválasztott érkezési és távozási dátum alapján.
- Szűrés szobatípus szerint (Standard, Deluxe, Suite).
- Szűrés elérhetőség szerint (Elérhető / Nem Elérhető).
- Kulcsszavas keresés szobaszám vagy leírás alapján.
- Ár szerinti szűrés (minimum és maximum érték megadása).
- Foglalás indítása elérhető szobákra.


## 🗓️ Dátumválasztás

- **Érkezési dátum**: minimum 8 nappal a mai nap után választható.
- **Távozási dátum**: legalább 1 nappal az érkezési dátum után választható.
- Ha a dátumok érvénytelenek, figyelmeztető üzenet jelenik meg.


---
## 🔍 Szűrési lehetőségek

### 🛏️ Szobatípus
- Standard
- Deluxe
- Suite

![Hiba képernyőképe](/roomtypes.png)

### 🚦 Elérhetőség
- Elérhető
- Nem elérhető

![Hiba képernyőképe](/roomavailability.png)


### 💬 Keresés
- Kulcsszavas keresés a szobaszám vagy leírás alapján.

### 💰 Ár szűrés
- Minimum ár (pl. 0 HUF)
- Maximum ár (pl. 300000 HUF)

---

## 📦 Szobakártyák tartalma
- Szobaszám (#)
- Szobatípus
- Leírás
- Ár / éjszaka (HUF)
- Ágyak száma (kapacitás)
- Elérhetőségi státusz
- "Foglalás" gomb (csak elérhető szobák esetén aktív)

---

## 🧭 Navigáció
- A "Foglalás" gomb kattintására átirányítás történik a foglalási oldalra, a kiválasztott szoba adatainak átadásával.

---

## ⚠️ Lehetséges hibák

#### 1. Hibás dátumválasztás
**Hibaüzenet:** „A távozási dátumnak későbbinek kell lennie, mint az érkezési dátum. Kérjük, módosítsa a dátumokat.”

![Hiba képernyőképe](/InvalidSearch.png)

#### 2. Nincs találat a szűrők alapján
**Hibaüzenet:** „Nincs elérhető szoba a megadott szűrők alapján. Próbálja meg módosítani a keresési feltételeket.”

![Hiba képernyőképe](/InvalidSearch.png)

#### 3. Hálózati / szerverhiba
**Hibaüzenet:** „Failed to fetch rooms.”

.
---
.
`,
  contact: `
## ✉️ Contact (Kapcsolatfelvétel)

### 📋 Leírás:
Lehetőség üzenetet küldeni a CozyNest csapatának.

### ✅ Funkciók:
- Név, e-mail, üzenet mezők kitöltése.
- Küldés gomb.

### ⚠️ Lehetséges hibák:
- Üres mezők:  
  **Hibaüzenet:** „Kérjük, töltsön ki minden mezőt.”  
  <div className="image-container">📷 *(Ide jön a hiba képernyőképe)*</div>
- Hibás e-mail formátum:  
  **Hibaüzenet:** „Érvénytelen e-mail cím.”  
  <div className="image-container">📷 *(Ide jön a hiba képernyőképe)*</div>
- Sikertelen küldés:  
  **Hibaüzenet:** „Az üzenet elküldése nem sikerült. Kérjük, próbálja újra később.”  
  <div className="image-container">📷 *(Ide jön a hiba képernyőképe)*</div>


`,
  reserve: `
## 🛏️ Szoba foglalása

### 📋 Mire való ez az oldal?
Ezen az oldalon lehetőséged van kiválasztott szobát lefoglalni a kívánt időszakra, megadni hány vendég érkezik, valamint extra szolgáltatásokat is kérhetsz a tartózkodás idejére.

---

### ✅ Mit tudsz itt csinálni?

- 👉 Kiválaszthatod az érkezés és távozás dátumát
- 👥 Megadhatod, hány vendég érkezik
- 🧴 Extra szolgáltatásokat kérhetsz (pl. reggeli, wellness)
- ✍️ Írhatsz megjegyzést a foglaláshoz
- 💰 Az ár automatikusan frissül, amit az oldal alján látsz
- ✅ A "Foglalás" gombra kattintva véglegesítheted a foglalást

---

### 🧰 Elérhető extra szolgáltatások:

*(A kiválasztott napokra, vendégenként számítjuk őket az árba)*

- **🍳 Prémium reggeli** – 5000 Ft/nap  
  Friss gyümölcslevek, kávé, bio finomságok

- **🍷 All-inclusive italcsomag** – 10 000 Ft/nap  
  Korlátlan üdítők, bor, koktél, tea, kávé

- **💆 Wellness & Spa belépő** – 8000 Ft/nap  
  Szauna, jacuzzi, gőzfürdő, relaxációs terek

- **🏋️ Edzőterem & sportprogramok** – 3000 Ft/nap  
  Jóga, személyi edző, modern felszerelések

- **🧼 VIP szobatakarítás** – 3000 Ft/nap  
  Extra tisztaság, prémium kozmetikumok

- **🚴‍♂️ Bicikli vagy roller kölcsönzés** – 4500 Ft/nap  
  Fedezd fel a környéket két keréken!

- **🏖️ Privát strand / napágy** – 7000 Ft/nap  
  Külön hely, kényelmes pihenéshez

- **🍽️ Gasztroélmény csomag** – 18 000 Ft/nap  
  Gourmet vacsora, helyi specialitások

- **🍲 Teljes ellátás** – 12 000 Ft/nap  
  Reggeli, ebéd és vacsora minden nap

---

### ℹ️ Fontos tudnivalók:

- Foglaláshoz be kell jelentkezned
- A megadott dátumok között legalább 1 éjszaka kell legyen
- A vendégek száma nem haladhatja meg a szoba kapacitását
- A foglalás véglegesítéséhez kattints a **"Foglalás"** gombra

---

### ⚠️ Hibaüzenetek, amikkel találkozhatsz:

- **„Nincs elérhető szoba a megadott feltételekkel.”**  
  – Válassz másik dátumot vagy kevesebb vendéget

- **„A kezdő dátum nem lehet későbbi a befejező dátumnál.”**  
  – Ellenőrizd a kiválasztott napokat

- **„Foglaláshoz kérjük, jelentkezzen be.”**  
  – Jelentkezz be a folytatáshoz

- **„Hiba történt a foglalás során.”**  
  – Próbáld újra vagy nézd meg az internetkapcsolatod
  
  .
`,
  reservations: `
## 📆 Foglalásaim

### 📋 Mire való ez az oldal?
Ezen az oldalon láthatod az eddigi és jövőbeni szállásfoglalásaidat, valamint lehetőséged van azokat megtekinteni vagy lemondani (ha még nem kezdődtek el).

---

### ✅ Mit tudsz itt csinálni?

- 📖 Megnézheted a szobáid adatait (típus, leírás, szám)
- 📅 Láthatod az érkezési és távozási időpontokat
- 📌 Ellenőrizheted a foglalás státuszát
- 📝 Elolvashatod a megadott megjegyzést (ha van)
- ❌ Lemondhatod a foglalást egy gombnyomással, ha az még nem aktív

---

### ℹ️ Mit jelentenek az egyes elemek?

- **Szoba:** a lefoglalt szoba száma (pl. 203)
- **Leírás:** a szoba jellemzői (pl. panorámás, deluxe ágy)
- **Érkezés / Távozás:** a tartózkodás kezdő és befejező dátuma
- **Státusz:** a foglalás állapota (pl. Aktív, Lemondva)
- **Jegyzet:** amit te adtál meg a foglalás során (pl. "Kérünk gyerekágyat")

---

### ⚠️ Lehetséges üzenetek és hibák:

- **„Nem található foglalás.”**  
  – Még nem hoztál létre egyetlen foglalást sem.

- **„Foglalások betöltése...”**  
  – A rendszer épp lekéri az adataidat.

- **„Nem sikerült betölteni a foglalásokat.”**  
  – Valami hiba történt az adatok lekérésekor, próbáld újra később.

- **Lemondás után a foglalás eltűnik a listából.**  
  – Ez azt jelenti, hogy sikeresen lemondtad.

---

### 🔐 Fontos:
A foglalásaid megtekintéséhez be kell jelentkezned a fiókodba.

.
---
.
.
`,
  profile: `
## 👤 Profil

### 📋 Leírás:
Felhasználói adatok megtekintése és szerkesztése.

### ✅ Funkciók:
- Név, e-mail, jelszó módosítása.
- Profilkép feltöltése (ha van ilyen funkció).
- Előfizetés kezelése (ha van ilyen funkció).

### ⚠️ Lehetséges hibák:
- Üres kötelező mező:  
  **Hibaüzenet:** „Minden mező kitöltése kötelező.”  
  ![Hiba képernyőképe](/loginError.png)
- Jelszó túl rövid vagy gyenge:  
  **Hibaüzenet:** „A jelszónak minimum 8 karakter hosszúnak kell lennie.”  
  ![Hiba képernyőképe](/loginError.png)
- Sikertelen mentés:  
  **Hibaüzenet:** „Invalid Access token.” (Lejárt a bejelentkezés     )  
  ![Hiba képernyőképe](/LoginExpired.png)
`,
  faq: `
## ❓ GYIK (Gyakran Ismételt Kérdések)

### 🔹 Hogyan tudok szobát foglalni?
1. Jelentkezzen be fiókjába.
2. Kattintson a "Szoba foglalása" menüpontra.
3. Válassza ki az időpontot, árkategóriát és férőhelyet.

### 🔹 Elfelejtettem a jelszavam. Mit tegyek?
Használja az „Elfelejtett jelszó” linket a bejelentkezési oldalon.

### 🔹 Lemondhatom a foglalásom?
Igen, a „Foglalásaim” menüpontban található „Lemondás” opcióval.

### 🔹 Milyen böngészők támogatottak?
- Google Chrome (legfrissebb verzió)
- Mozilla Firefox
- Safari
- Microsoft Edge
`,
  terms: `
## 📜 Felhasználási feltételek

A CozyNest használatával Ön elfogadja az alábbi feltételeket:

- Valós adatokat ad meg regisztrációkor.
- Nem használja a rendszert visszaélésszerűen.
- A foglalási feltételek változhatnak, erről értesítést kap.

A teljes feltételek elérhetők a hivatalos weboldalon.
`,
  privacy: `
## 🔐 Adatvédelmi nyilatkozat

- Adatokat kizárólag a szolgáltatás működéséhez gyűjtünk.
- Nem adjuk tovább harmadik félnek.
- Kérésre bármikor törölhetők az adatok.

Részletek: [Adatkezelési szabályzat](https://localhost/adatvedelem)
`,
  support: `
## 🛠️ Technikai támogatás

📧 E-mail: support@cozynest.hu  
📞 Telefon: +36 1 234 5678  
💬 Élő chat: elérhető a jobb alsó sarokban.

### 🕘 Ügyfélszolgálati idő:
- Hétfőtől Péntekig: 09:00 - 18:00
- Hétvégén: korlátozott elérhetőség

### 💡 Tipp:
Először tekintse meg a GYIK szekciót a gyors megoldásokért.
`,
  devdocs: `
## 🧑‍💻 Fejlesztői dokumentáció

---

### ⚙️ Futtatási követelmények

- **Node.js** (v18+ ajánlott)
- **.NET 7 SDK**
- **Visual Studio 2022** (backend/WPF)
- **Visual Studio Code** (frontend)
- **MySQL 8+**
- **Git**

---

### 🛠️ Telepítés

#### 📦 Backend (.NET 7 API)

\`\`\`bash
cd backend
dotnet restore
dotnet run
\`\`\`

#### 💻 Frontend (React)

\`\`\`bash
cd frontend
npm install
npm run dev
\`\`\`

#### 💥 WPF (CozyNestAdmin)

1. Nyisd meg Visual Studio 2022-ben
2. Állítsd be az API URL-t a \`GlobalMethods.cs\` fájlban
3. Buildeld és futtasd

---

### 💾 Adatbázis szerkezete és kapcsolatok

#### \`roles\`
- Felhasználói szerepkörök *(admin, vendég stb.)*

#### \`users\`
- Felhasználói fiókok: jelszó (argon2 hash), e-mail, név, cím, szerepkör ID
- 🔗 Kapcsolat: \`role_id → roles(id)\`

#### \`tokens\`
- Belépési tokenek (hozzáférési + frissítő)
- 🔗 Kapcsolat: \`user_id → users(id)\`

#### \`roomstatus\`
- Szobastátuszok *(elérhető, karbantartás alatt)*

#### \`roomtype\`
- Szobatípusok *(standard, deluxe, suite)*

#### \`room\`
- Szobák: szám, típus, ár, kapacitás, státusz
- 🔗 Kapcsolat: \`type → roomtype(id)\`, \`status → roomstatus(id)\`

#### \`reservationstatuses\`
- Foglalás állapotok *(aktív, lemondva, teljesült stb.)*

#### \`reservations\`
- Foglalások: vendég, szoba, időszak, megjegyzés
- 🔗 Kapcsolat: \`guest_id → users(id)\`, \`room_id → room(id)\`, \`status → reservationstatuses(id)\`

#### \`services\`
- Extra szolgáltatások *(reggeli, wellness stb.)*

#### \`reservationservices\`
- Kapcsolótábla foglalások és szolgáltatások között
- 🔗 Kapcsolat: \`reservation_id → reservations(id)\`, \`service_id → services(id)\`

---

### 🧰 Fejlesztői eszközök

- **ASP.NET 7 Web API**
- **React + TypeScript**
- **MySQL + mysql.data**
- **Argon2** jelszókódolás

---

### 🖥️ Fejlesztési környezet

- **VS Code** (frontend): ESLint, Prettier, vite
- **Visual Studio 2022** (backend/WPF)
- **DBeaver / MySQL Workbench** (adatbázis)

---

### 🔄 Fejlesztés menete

1. 🧱 Adatmodell tervezése *(MySQL)*
2. 🛡️ Backend API fejlesztés és validációk
3. 🎨 Frontend komponensek és állapotkezelés
4. 🖥️ WPF admin interfész *(CozyNestAdmin)*
5. 🧪 Integrációs tesztelés
6. 🐞 Hibakezelés és UX finomhangolás
7. 📝 Dokumentáció és verziókezelés *(Git)*








.
---
.
.
.
`,
};

const Documentation = () => {
  const [activeSection, setActiveSection] = useState("home");

  return (
    <div className={style.container}>
      <aside className={style.sidebar}>
        <h2>Dokumentáció</h2>
        <ul>
          {sections.map((section) => (
            <li key={section.id}>
              <button
                className={
                  activeSection === section.id ? style.activeLink : style.link
                }
                onClick={() => setActiveSection(section.id)}
              >
                {section.label}
              </button>
            </li>
          ))}
        </ul>
      </aside>
      <main className={style.content}>
        <ReactMarkdown
          components={{
            img: ({ node, ...props }) => (
              <img
                {...props}
                style={{
                  maxWidth: "100%",
                  width: "300px",
                  borderRadius: "8px",
                }}
                alt={props.alt}
              />
            ),
          }}
        >
          {markdownContent[activeSection]}
        </ReactMarkdown>
      </main>
    </div>
  );
};

export default Documentation;
