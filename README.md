# StudyHub

### 👤 Integrantes:
- André Alves Toledo Silva, R.A: 22.122.020-5
- Arthur Carvalho Rotkis, R.A: 22.123.116-0
- Cauê Iwamoto Meira, R.A: 22.221.039-5
- Hanna de Oliveira Melo, R.A: 22.223.002-1
- Sérgio de Siqueira Santos, R.A: 22.124.082-3

### 📚 Descrição:
Será inserido no domínio educacional e consiste em uma plataforma colaborativa para compartilhamento e comercialização de projetos, materiais e conteúdos educacionais. Nesta plataforma, os usuários poderão publicar e consultar projetos, aulas, cursos e materiais de estudo. Permitindo também a criação de grupos, nos quais os integrantes poderão compartilhar uma agenda para organizar atividades, reuniões, estudos e outras tarefas.

### 🎯 Objetivo Principal do Sistema:
Desenvolver uma plataforma educacional que facilite o acesso, o compartilhamento e a organização de conteúdos e projetos, permitindo também a colaboração entre os usuários por meio de grupos e agendas compartilhadas.

### 👥 Usuários finais do Sistema:
| **Usuário** | **Descrição** |
| -------- | -------- |
| Estudante | Pode consultar, adquirir e avaliar os conteúdos, além de participar de grupos de estudos e utilizar suas agendas. |
| Autor | Pode publicar projetos, aulas, cursos e materiais educacionais na plataforma. |

## 🔄 Oportunidades de Reuso

O StudyHub utiliza diferentes soluções e tecnologias reutilizáveis para reduzir o esforço de desenvolvimento, aumentar a produtividade e facilitar a manutenção do sistema.

As principais oportunidades de reuso identificadas são:

- **Next:** desenvolvimento das interfaces web e mobile por meio de componentes reutilizáveis.
- **JWT:** autenticação e controle de acesso dos usuários.
- **shadcn/ui:** utilização de componentes reutilizáveis para construção e padronização das interfaces, incluindo a agenda compartilhada.
- **Serviço de Chat:** comunicação entre usuários e integrantes dos grupos.
- **Firebase:** armazenamento e disponibilização dos arquivos e projetos educacionais.
- **API de Pagamento:** processamento das aquisições de conteúdos disponibilizados na plataforma.

A utilização dessas soluções permite que a equipe concentre seus esforços no desenvolvimento das funcionalidades específicas do StudyHub, evitando a implementação de recursos comuns do zero.

**Projeto da disciplina Tópicos Avançados de Engenharia de Software - FEI, 2º semestre 2026**

### Diagrama de Componentes
<img width="1041" height="791" alt="Diagrama sem nome" src="https://github.com/user-attachments/assets/85672ba0-df98-446c-8f8c-9b7a6204fc73" />
[Link do Diagrama DrawIO](https://viewer.diagrams.net/?tags=%7B%7D&lightbox=1&highlight=0000ff&edit=_blank&layers=1&nav=1&dark=auto#R%3Cmxfile%3E%3Cdiagram%20name%3D%22P%C3%A1gina-1%22%20id%3D%22ulu5WSF4bMv5usVPVDG_%22%3E7VzbcqM4EP0aPyaFuPOYOJnMVu1WpSp7mXlUQLE1wZZXyGM7X78CJBsJbGMHkXiWlwQaSUD36aPulvDIGc%2FWDxQupn%2BQBKUj20rWI%2BduZNs2cB3%2BL5dsSgmwHL%2BUTChOhGwneMJvSDYU0iVOUKY0ZISkDC9UYUzmcxQzRQYpJSu12QtJ1bsu4ATVBE8xTOvSf3DCpqU0tIOd%2FCvCk6m8M%2FCj8soMysbiTbIpTMiqInLuR86YEsLKo9l6jNJce1IvZb8ve65uH4yiOWvTYZXcv93F%2F26%2BWrP47%2BfZ4%2Byb9XbluPVhxMgZ20glTNks5Udg5NwmlCz%2BhHSC8sYWF6ymmKGnBYzztiuOAi4rB%2FgJ06UY4AHx4WMMZ%2FwmhF965KOQjEHREFGG1pWbixd4QGSGGN3wJtOKjiOh0NXOHiAUMgk8%2BV4Cdl4grkMBh8l26J3G%2BIFQ2ikK9OoKPKjpilo5Ihb54Ywky1x2%2BwMxtpEwC6Vgiy%2B3SbVnqVBikqIUMvxTxXpFr7aqVrBfieJ%2BjwRzLWw7XEk%2FEXYI1AHIy0vGgaTbYPvQ7zCL3wuu%2F8qWo7EzugEUE1NYDlyrLywHdaUd1O6A5V6wHPaC5UeU4MQYit3AvvYU%2FTmWMRxHbXEcDjjuD8eSxwzj%2BAnRn7hg5YBkpuCsBxih6xkCs9sQ6B1U7wDmXsBs90PKcCKDZjNAvgJqdBEAU6wsU8HjQLYHIPcI5H4i5Rt%2BjGFJy8Vfx2CooQYawDIWabhtI2Z3iJj7xHQ%2FEbNe1UgQ%2F1MNP8whXA%2BmDdJ222DaHYLpHiHuCfujpFa%2FrGOeLGmMDgzWkAdRspwnKBHuQCibkgmZw%2FR3QhbCaQrbiYotXHIPcG4rLoXmyU1ef%2BWnhFuzlHzB%2BVsWQ2aviMVTeVI84SOimOsF0dzx8HwiLrLCMfdc5IOKRwDF6Rqzb%2FIJ%2BPH3vBn3lfLsbi175ScbcVK%2BO5Puv1fjfiM06zhsxFx7SPl%2BLVPWkwuhrrKfAWy1Cgn4QHiRoT1kWUECn%2Fgnc34c88EQzQXZoizVv%2BB1DrHbFw6LMUkJv3g3J%2FN8zIxR8oo0IUUZfoPPxRPklltITGxHf9xJbilhkFVawzQlqxu5JlD0zzVY6NS7HXl3OdCXLMVzfl%2B5mgA6oyTQwOt6pa9ueVO07jVELnUb8%2FdjqjGbbN1oqwab1oCQaxDHML0RF2Y4SYp5QmWfmv5%2Fe0etyqmbwdfMAALdDJ4pM%2FgHWHtwNXOuBixbzXt9YwGU36pS8Hk9jfLxDXmaF6pGcCNjRmi1zjn4Wdd%2BFta4FFjGSqV%2Bw1rs5Xja%2FZxRNDlvSfy4q9mRrZnBM2YGkTF2k5rUCyddpiZTmL6MMY0LC6kJyi6z8Ld4kKkr0BMYmXfI4zZ5B9cF3RSdriPgSEHeE1xzx5GCXefiTOld4YZTUpkANPr%2Fe1MZrlS4qTQQXLQ30%2FF8R5uDPQ185Yj7euvRsquHaXvypIahtCV2PRgQKaiphEuu6A%2FJfGMyv3Ok76Ndav%2BuZN6QB7QBqY4tw8l8MGQYHxL5eJ4e%2BXjGliGCi04xxlPIDEU9jqcu05srkwdOhxQuF7AuJei5jkIl7vHl6Z7Ah580Ry69BSR7Q4HI0wk7sttVX2tDbatGEnx6xG06qmiVDg3M3zXzOyDS7G6MdPwOSadh%2Fe5Tk84JmVbboDDsJyh0gTYr6cRgOiQMLmeu%2Boi1w2unm9ns0wDO8Y%2FMRKYB12o%2FxjATdT4Thb3NRF3W%2FBp2slYYQZiuggeVbyplNU8pqh0rqDVWG6yDvt%2FSz%2BVm4aN%2BXjWvtNWxLSyHbNpMByBS6SC0z6yb2dqnKYH%2BaYrhCFcuHxne5%2FWeyvwZW3AN7lcM224mD%2BsuOGzmMraZK7S7ZE%2F7EHs%2BpyR%2BzT0BZtOCMsGBcBzYB4n2DMLckXOVmitMvYecuybaY1AC%2FjFWPXWxQ1vqPm2tA1ha7%2FBcxvaPDGSasVutw3%2FWSqXZJVptVo4sUwu0YZd7Rxtmik9dNjg5uvvgRVKNNyL1K%2F8jvOHYejUTgHPLmXsepDfm6LTWVZ8jL7xocfoaqemiA3A9vZKux7WXAr1We2WH8kXnmzRdPTEyVr8Iww7ZpeGnPv4%2FEfi7SppGI3DHVsvu8rOKliF4oNU69PCs9bKg9hi1nceG2SwaNoR8CJvZrlpdMLgRVu717CZW%2BnV3YPYTCLkWODKNtacOfULUIWSaO7rc5WI4c7yIrw5NY8%2BWgc0vgL3hE44P2s7SwbzFT3e%2FdFgiYveDkc79fw%3D%3D%3C%2Fdiagram%3E%3C%2Fmxfile%3E)
