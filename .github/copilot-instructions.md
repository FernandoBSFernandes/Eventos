# Copilot Instructions

## Diretrizes de projeto
- O usuário prefere evitar APIs, pacotes e abordagens depreciadas (deprecated) durante qualquer alteração no projeto e quer remover tudo que estiver depreciado e atualizar bibliotecas quando necessário.
- O usuário prefere que o código seja escrito em português, mas pode aceitar comentários em inglês se necessário.
- O usuário prefere que a atualização dos pacotes seja feita levando em consideração a compatibilidade com a versão utilizada no .NET no projeto.
- O usuário prefere que sempre que houver criação ou edição de código, seja criado, executado os testes e criado novos casos de testes de carga e performance, caso necessário. Além disso, ao realizar qualquer alteração no código, considerar e validar todos os testes existentes na solution.
- O usuário quer que o código seja arquitetado com SOLID, DDD, Clean Architecture e TDD, sempre que possível.
- Se necessário, usar padrões de projeto (Design Patterns) para resolver problemas de arquitetura e código.
- Deixar o código bem organizado em pastas e namespaces, seguindo boas práticas de organização de código.
- O usuário deseja que os arquivos criados tenham encodamento UTF-8, sem BOM, e que o código seja escrito com a cultura pt-BR.
- O usuário quer apagar a pasta .vs, a pasta bin e obj e arquivos .user nas limpezas/ações do repositório e ignorar essas pastas e arquivos no .gitignore.
- Não criar script local para executar a esteira completa quando a pipeline do GitHub já cobre build, testes e validações.

## Módulo de Relatórios
- O endpoint pessoa-mesa deve gerar um PDF próprio (relação pessoa x mesa), diferente do PDF de lista-final com checkbox de pagamento; e os relatórios devem permanecer em PDF.
