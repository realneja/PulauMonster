# Pulau Monster

Praktikum Proyek Gim 2D (TP301) pertemuan P1 sampai P7, D4 Teknologi Permainan, Politeknik Negeri Batam.

| | |
|---|---|
| Nama | Neza Insyirah Nasuha |
| NIM | 4352511002 |
| Kelas | TP 3A Malam |
| Branch | NezaInsyirahNasuha_4352511002 |

## Cara membuka

1. Buka folder ini lewat Unity Hub dengan Unity 6000.6.5f1.
2. Tekan Play. Scene apa pun yang sedang terbuka, game selalu mulai dari Scn_Bootstrap lalu masuk ke layar judul.

## Kontrol

| Tombol | Fungsi |
|---|---|
| WASD atau panah | Jalan, pindah pilihan menu |
| Shift kiri | Lari |
| E, Spasi, Enter | Bicara, memilih, lanjut dialog |
| Esc | Menu jeda, batal |

## Isi tiap pertemuan

| Pertemuan | Materi | Yang ada di proyek |
|---|---|---|
| P1 | Setup proyek dan gerak karakter | Scene pribadi Scn_Neza, gerak 4 arah, lari x1,6, tembok batas dan rintangan |
| P2 | Arsitektur dan world tilemap | Scn_Bootstrap, GameServices, peta 40 x 36 tile dengan collider |
| P3 | Animasi, kamera dan interaksi | Animasi 4 arah, kamera Cinemachine dengan confiner, sapa NPC dengan E, nama area |
| P4 | Data monster dan monster liar | Data monster dari CSV, monster liar dengan state machine, Scn_Battle |
| P5 | Battle dan UI | Menu battle, jurus elemen, Bertahan, Ramuan, Kabur, naik level, evolusi |
| P6 | Dialog, save dan audio | Layar judul dengan dua slot save, dialog bercabang, Batu Simpan, menu jeda, musik per area |
| P7 | Polish, build dan demo | Cahaya di Gua Petir, partikel, efek battle, layar DEMO SELESAI |

## Struktur folder

Semua isi game ada di `Assets/_Project`. Folder `StarterAssetPack` berisi paket aset dari dosen. Builder di menu **Pulau Monster** memakai folder itu untuk menyusun ulang tiap pertemuan.
