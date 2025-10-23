# Git Hooks

Questo progetto utilizza **git hooks globali** configurati in `/data/repos/.git-hooks/`.

## Hooks Attivi

### Pre-Commit Hook
Esegue automaticamente prima di ogni commit:
- ✅ **Security check**: Rileva secrets, API keys, chiavi private
- ✅ **Code formatting**: Verifica `dotnet format` compliance

### Pre-Push Hook ⭐ NUOVO
Esegue automaticamente prima di ogni push:
- ✅ **Unit tests**: Esegue solo i test unitari (veloci, 2-5 secondi)
- ⏭️ **Integration tests**: Skippa i test di integrazione (lasciati alla CI)

**Caratteristiche:**
- 🚀 **Veloce**: Solo unit test, non integration test
- ⚡ **Non invasivo**: Non rallenta i commit, solo i push
- 🎯 **Smart**: Rileva automaticamente se è un progetto .NET
- 🔓 **Bypassabile**: Usa `git push --no-verify` in emergenza

## Come Funziona

```bash
# Workflow normale con commit multipli
git commit -m "WIP: prima parte"        # ✅ Veloce (pre-commit: security + format)
git commit -m "WIP: seconda parte"      # ✅ Veloce
git commit -m "feat: completato"        # ✅ Veloce

# Quando sei pronto a pushare
git push origin develop                 # 🧪 Esegue unit tests

# Se test falliscono
❌ Push bloccato → fix → push again

# Se test passano
✅ Push completato → CI remota esegue test completi (multi-OS)
```

## Bypass Hook (Emergenze)

Se devi pushare urgentemente senza test:

```bash
git push --no-verify origin develop
```

⚠️ **Attenzione**: La CI remota eseguirà comunque tutti i test.

## Struttura

- **Hook globali**: `/data/repos/.git-hooks/` (attivi per tutti i repository)
- **Template versionati**: `.github/hooks/` (questo repository, per reference)

## Configurazione Globale

Gli hook globali sono già configurati via:

```bash
git config --global core.hooksPath /data/repos/.git-hooks
```

Tutti i repository in `/data/repos/` usano automaticamente questi hook.

## Test Strategy

| Dove | Cosa | Quando | Tempo |
|------|------|--------|-------|
| **Pre-Push (locale)** | Unit tests | Prima di push | 2-5 secondi |
| **CI GitHub Actions** | Tutti i test | Dopo push | 3-5 minuti |
| | | 3 OS (Ubuntu/Windows/macOS) | |

## File Rilevanti

- `pre-push` - Script hook principale
- `/data/repos/.git-hooks/pre-commit` - Orchestrator per security + format
- `/data/repos/.git-hooks/pre-commit.d/01-security` - Secrets detection
- `/data/repos/.git-hooks/pre-commit.d/02-dotnet-format` - Code style check

## Vantaggi

✅ **Feedback immediato**: Cattura problemi prima della CI
✅ **Non invasivo**: Solo al push, non ad ogni commit
✅ **Veloce**: Solo unit test (integration test → CI)
✅ **Multi-piattaforma**: CI testa su tutti gli OS
✅ **Flessibile**: Bypassabile in emergenza

---

*Ultimo aggiornamento: 2025-10-18*
