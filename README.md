# Baskı Envanteri (Print Inventory)

A Windows desktop app to track 3D printer filament spools, 3D models and print jobs.
It calculates how much filament is left on each spool and what each print cost.

Built with C# / .NET 10 / WinForms, Entity Framework Core and SQLite.
The user interface is in Turkish, with a dark "neon" design that is drawn in code
(GDI+, no UI or chart libraries). The code itself is in English.

## Requirements

- Windows 10 or 11 (dark title bars and dark scroll bars need Windows 10 1903 or newer)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## How to run

From the solution folder (the folder with `PrintInventory.slnx`):

```
dotnet run --project PrintInventory
```

Or open `PrintInventory.slnx` in Visual Studio and press F5.

On the first start the app creates its database automatically and adds one printer
(Creality Ender 3 S1).

## Where the data is stored

Everything is under `%LocalAppData%\PrintInventory`:

| Path | Content |
|---|---|
| `printinventory.db` | The SQLite database |
| `Images\` | Copies of the model preview images (the database stores only their file paths) |

To start with an empty database, close the app and delete `printinventory.db*`.
To make a backup, close the app and copy the whole folder.

## The interface

One main window, no tabs:

- **Top bar**: printer chip, **+ Baskı** (new print job), **+ Makara** (new spool), **☰ Yönet** (manage).
- **Summary tiles**: filament left, active spools, low stock, number of prints, success rate (ring).
- **Makara rafı** (spool rack): one card per active spool. The ring is drawn in the filament's
  real color and shows how much is left; low-stock spools glow red.
  Click a card to open its **detail drawer**: facts, adjust remaining, edit, archive, its prints.
- **Son baskılar**: the last print jobs. **Stoktaki malzemeler**: donut chart of filament per material.
- **☰ Yönet** opens a drawer with tabs: Filaments, Models (picture gallery), Printers,
  full print history (with delete), and archived spools (unarchive).

Esc or a click on the darkened background closes a drawer.

## Business rules

- Remaining grams is **calculated, not stored**:
  `Remaining = InitialWeightGrams - sum of the spool's print job UsedGrams + AdjustmentGrams`
- "Kalanı düzelt" (adjust remaining) saves a new `AdjustmentGrams`, so the formula gives the weighed value.
- A new print job uses the model's estimated grams by default; the value can be changed.
- If a job uses more grams than the spool has left, the app warns but still allows saving.
- Spools are archived, never deleted, so the print history stays complete.
- Print cost = `PricePaid / InitialWeightGrams * UsedGrams`.
- Success rate = successful / (successful + failed) prints; cancelled prints are not counted.

## Project structure

```
PrintInventory/
  Data/       Entity classes, AppDbContext, AppPaths, EF Core migrations
  Services/   Business rules (SpoolService, PrintJobService, DashboardService, ImageStorageService)
  Forms/      User interface only
                MainForm            the single main window
                SpoolDetailPanel,   contents of the sliding drawer (SideDrawer)
                ManagePanel
                *EditForm           add/edit dialogs (layout helpers in UiTheme)
                DarkTheme           colors, fonts and drawing helpers (rings, cards, glow)
                SpoolCard, StatTile, RingView, MaterialDonut, ...   self-drawn controls
                DisplayText         Turkish texts for enum values and formatting
  Program.cs  Start: Turkish culture, applies migrations, opens MainForm
```

Values like `Success` or `PLA` are stored in English in the database and shown in
Turkish through `DisplayText`, so the data does not depend on the UI language.

## Changing the database schema

The schema is managed with EF Core migrations. The `dotnet-ef` tool is installed as a local tool
(see `dotnet-tools.json`), so run `dotnet tool restore` once after cloning.

1. Change or add a property in an entity class in `Data/`.
2. Create a migration:
   ```
   dotnet ef migrations add <DescriptiveName> --project PrintInventory -o Data/Migrations
   ```
3. Start the app. It applies new migrations automatically at startup (`Database.Migrate()`).

Never edit a migration that has already been applied; add a new one instead.
