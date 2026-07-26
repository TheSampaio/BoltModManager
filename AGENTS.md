# Bolt Mod Manager

## Project principles

- Keep the application generic: game-specific behavior must not be added to the product code.
- Follow clean code, SOLID, DRY, and KISS. Prefer focused changes over new abstractions without a demonstrated need.
- Preserve the layer boundaries documented in `README.md`:
  - `Core` contains domain models and abstractions and must not depend on UI or file-system implementations.
  - `Infrastructure` owns storage, archives, symbolic links, elevation, and other operating-system integration.
  - `Services` contains application workflows.
  - `UI` contains forms, controls, themes, and presentation behavior.
- Keep `Program.cs` as the only composition root. Use dependency injection and do not introduce a global service provider.
- Application services return results; only the UI displays dialogs.
- Preserve Bolt's non-destructive deployment model: modifications remain outside the game and are linked into it, while replaced originals are recoverable from backups.

## Versioning

- Before completing any functional change, bug fix, or visible UI change, update `App:Version` in `Bolt/appsettings.json`.
- Use the calendar version format `YYYY.M.D.REVISION`.
- For the first version on a new local date, use revision `1`. For another version on the same date, increment the revision.
- Documentation-only changes do not require a version update unless they change user-facing application behavior.

## Validation

- Build `Bolt.sln` after code changes and resolve all build errors and warnings introduced by the change.
- Do not launch games or modify a user's real game/mod directories unless the user explicitly asks for that test.
