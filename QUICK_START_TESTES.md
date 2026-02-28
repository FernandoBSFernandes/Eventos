# ? QUICK START: Testes em 30 Segundos

## ?? Opção 1: Mais Rápido (Recomendado)

```bash
dotnet test
```

**Pronto!** ? Testes rodam e você vê o resultado em segundos.

---

## ?? Opção 2: Com Opções (PowerShell - Windows)

```powershell
# Testes simples
.\testes.ps1

# Com cobertura
.\testes.ps1 -Coverage

# Contínuo (watch)
.\testes.ps1 -Watch

# Tudo
.\testes.ps1 -Coverage -Verbose -Watch
```

---

## ?? Opção 3: Com Opções (Bash - Linux/Mac)

```bash
# Testes simples
bash testes.sh

# Com cobertura
bash testes.sh --coverage

# Contínuo (watch)
bash testes.sh --watch
```

---

## ?? Opção 4: Visual Studio

1. Abra: `Ctrl + E, T` (Test Explorer)
2. Clique: **Run All** ??
3. Veja: Resultados em tempo real

---

## ?? Resultado Esperado

```
? Total: 15 testes
? Aprovados: 15
? Falhados: 0
?? Duração: ~3.6s
```

---

## ?? Automação Contínua

Testes rodam **automaticamente** no GitHub a cada push! ??

Veja em: `https://github.com/seu-usuario/Eventos/actions`

---

**Pronto! É só isso! ??**

Para mais detalhes:
- `README_TESTES.md` - Documentação completa
- `EXEMPLOS_PRATICOS_TESTES.md` - Exemplos de uso
- `TESTES_AUTOMATIZADOS_RESUMO.md` - Resumo visual
