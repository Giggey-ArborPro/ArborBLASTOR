# ArborBLASTOR

A simple command-line tool for sending Apple Push Notification service (APNs) alerts using JWT (`.p8` key) authentication.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An Apple `.p8` Auth Key file (`AuthKey_<KeyId>.p8`)
- A valid APNs device token from a registered device

---

## Configuration

The following APNs credentials are baked into the binary. To change them, edit `Program.cs`:

| Field      | Description                               |
|------------|-------------------------------------------|
| `CertFilePath` | Path to your `.p8` Auth Key file      |
| `KeyId`    | The 10-character Key ID from Apple        |
| `TeamId`   | Your Apple Developer Team ID              |
| `BundleId` | Your app's bundle identifier              |

The `.p8` file must be placed next to the compiled binary (it is copied automatically on build).

---

## Usage

```
ArborBLASTOR [options]
```

### Options

| Flag                       | Description                                              |
|----------------------------|----------------------------------------------------------|
| `--token <value>`          | APNs device token to send to                             |
| `--title <value>`          | Notification title                                       |
| `--body  <value>`          | Notification body                                        |
| `--push-type <type>`       | Push type to use (see [Push Types](#push-types) below)   |
| `--cert <path>`            | Path to `.p8` auth key file (see [below](#cert-file))    |
| `--list-push-types`        | Print all available push types and exit                  |
| `--help`, `-h`             | Show help message and exit                               |

If any of `--token`, `--title`, `--body`, or `--push-type` are omitted, you will be prompted to enter them interactively. The interactive push-type prompt presents a numbered list to choose from.

---

## Cert File

By default ArborBLASTOR looks for `AuthKey_D8T6LX3N8V.p8` **next to the compiled binary** (it is copied there automatically on build).

Use `--cert` to point to a different `.p8` file at runtime:

```powershell
.\ArborBLASTOR --cert "C:\keys\AuthKey_MYKEY.p8" --token "abc123" --title "Hello" --body "World"
```

If the resolved cert file does not exist, the tool will print a clear error and exit rather than crashing with a cryptographic exception.

---

## Push Types

| # | Type           | Description                                                  |
|---|----------------|--------------------------------------------------------------|
| 1 | `Alert`        | Standard visible notification with title and body *(default)*|
| 2 | `Background`   | Silent background wake — no UI shown to the user             |
| 3 | `Voip`         | VoIP call notification (requires VoIP token via `AddVoipToken`) |
| 4 | `Location`     | Location push (used with significant-location updates)       |
| 5 | `LiveActivity` | Live Activity update notification                            |
| 6 | `Mdm`          | Mobile Device Management payload                             |

List them at any time with:
```powershell
.\ArborBLASTOR --list-push-types
```

---

## Examples

**Specify push type via flag:**
```powershell
.\ArborBLASTOR --token "abc123def456" --title "Hello" --body "World" --push-type Alert
```

**Use a custom .p8 cert file:**
```powershell
.\ArborBLASTOR --cert "C:\keys\AuthKey_MYKEY.p8" --token "abc123def456" --title "Hello" --body "World"
```

**List all push types:**
```powershell
.\ArborBLASTOR --list-push-types
```

**Fully non-interactive:**
```powershell
.\ArborBLASTOR --token "abc123def456" --title "Hello" --body "World"
```

**Partially interactive** (prompted for title and body):
```powershell
.\ArborBLASTOR --token "abc123def456"
```

**Fully interactive** (prompted for all values):
```powershell
.\ArborBLASTOR
```

**Show help:**
```powershell
.\ArborBLASTOR --help
```

---

## Building & Running

```powershell
# Build (current platform)
dotnet build ArborBLASTOR/ArborBLASTOR.csproj

# Build for Apple Silicon Mac
dotnet build ArborBLASTOR/ArborBLASTOR.csproj -r osx-arm64

# Publish self-contained binary for Apple Silicon Mac
dotnet publish ArborBLASTOR/ArborBLASTOR.csproj -r osx-arm64 -c Release --self-contained

# Run directly via dotnet
dotnet run --project ArborBLASTOR/ArborBLASTOR.csproj -- --token "abc123" --title "Hello" --body "World"

# Or run the compiled binary
.\ArborBLASTOR\bin\Debug\net10.0\ArborBLASTOR.exe --token "abc123" --title "Hello" --body "World"
```

---

## Notes

- Push notifications are sent to the **development** APNs server. Remove or change `push.SendToDevelopmentServer()` in `Program.cs` for production.
- The device token must come from a device registered with the same bundle ID and environment (development/production).

