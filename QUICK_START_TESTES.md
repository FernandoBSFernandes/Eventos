# ? QUICK START: Testes em 30 Segundos

## ?? Op��o 1: Mais R�pido (Recomendado)

```bash
dotnet test
```

**Pronto!** ? Testes rodam e voc� v� o resultado em segundos.

---

## ?? Op��o 2: Com Op��es (PowerShell - Windows)

```powershell
# Testes simples
.\testes.ps1

# Com cobertura
.\testes.ps1 -Coverage

# Cont�nuo (watch)
.\testes.ps1 -Watch

# Tudo
.\testes.ps1 -Coverage -Verbose -Watch
```

---

## ?? Op��o 3: Com Op��es (Bash - Linux/Mac)

```bash
# Testes simples
bash testes.sh

# Com cobertura
bash testes.sh --coverage

# Cont�nuo (watch)
bash testes.sh --watch
```

---

## ?? Op��o 4: Visual Studio

1. Abra: `Ctrl + E, T` (Test Explorer)
2. Clique: **Run All** ??
3. Veja: Resultados em tempo real

---

## ?? Resultado Esperado

```
? Total: 15 testes
? Aprovados: 15
? Falhados: 0
?? Dura��o: ~3.6s
```

---

## ?? Automa��o Cont�nua

Testes rodam **automaticamente** no GitHub a cada push! ??

Veja em: `https://github.com/seu-usuario/Eventos/actions`

---

**Pronto! � s� isso! ??**

Para mais detalhes:
- `README_TESTES.md` - Documenta��o completa
- `EXEMPLOS_PRATICOS_TESTES.md` - Exemplos de uso
- `TESTES_AUTOMATIZADOS_RESUMO.md` - Resumo visual
