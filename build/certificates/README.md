# SheetAtlas Code Signing Certificates

## ⚠️ Development Certificate (Self-Signed)

This directory contains a **self-signed certificate** for development and testing purposes only.

### Files

- `SheetAtlas-CodeSigning.crt` - Public certificate
- `SheetAtlas-CodeSigning.key` - Private key
- `SheetAtlas-CodeSigning.pfx` - Windows format (PKCS#12) with password

### Password

```
sheetatlas-dev
```

### Usage

**For Inno Setup signing:**
```ini
SignTool=signtool sign /f "$qbuild\certificates\SheetAtlas-CodeSigning.pfx$q" /p "sheetatlas-dev" /tr http://timestamp.digicert.com /td sha256 /fd sha256 $f
```

**For direct signing with signtool:**
```powershell
signtool sign /f "SheetAtlas-CodeSigning.pfx" /p "sheetatlas-dev" /tr http://timestamp.digicert.com /td sha256 /fd sha256 "SheetAtlas-Setup.exe"
```

## ⚠️ Security Warning

**This certificate is self-signed and provides NO security validation.**

When users run the installer signed with this certificate:
- ✅ Windows Defender will see a valid signature (technically)
- ❌ SmartScreen will show "Unknown Publisher" warnings
- ❌ Users will see "Do you want to allow this app from an unknown publisher?"

### For Production Release

**You MUST replace this with a commercial certificate:**

1. **Standard Code Signing** ($100-300/year)
   - Providers: Sectigo, Certum, DigiCert
   - Still shows SmartScreen warnings initially
   - Reputation builds over time

2. **EV Code Signing** ($300-600/year) ⭐ **Recommended**
   - No SmartScreen warnings from day 1
   - Requires hardware token (included)
   - Higher validation standards
   - Instant trust

### Certificate Validation

Check certificate details:
```bash
openssl x509 -in SheetAtlas-CodeSigning.crt -text -noout
```

View PFX contents:
```powershell
certutil -dump SheetAtlas-CodeSigning.pfx
```

---

**Valid for:** 365 days from creation
**Created:** October 2025
**Subject:** CN=SheetAtlas Code Signing, O=SheetAtlas, L=City, ST=State, C=IT
