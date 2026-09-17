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
<img width="1041" height="791" alt="image" src="https://github.com/user-attachments/assets/9c89610d-03cb-44f7-b6c7-8d2b26f81317" />
[Link do Diagrama DrawIO](https://viewer.diagrams.net/?tags=%7B%7D&lightbox=1&highlight=0000ff&edit=_blank&layers=1&nav=1&dark=auto#R%3Cmxfile%3E%3Cdiagram%20name%3D%22P%C3%A1gina-1%22%20id%3D%22ulu5WSF4bMv5usVPVDG_%22%3E7Vzfc6M2EP5r%2FJgM4jePiZPLdaadyUx67d2jAoqtBluukM92%2FvoKkGwksI0dROIrLwkskoDdbz%2FtroRHzni2fqBwMf2DJCgd2VayHjl3I9u2gevwf7lkU0qA5filZEJxImQ7wRN%2BQ7KhkC5xgjKlISMkZXihCmMyn6OYKTJIKVmpzV5Iqt51ASeoJniKYVqX%2Fo0TNi2loR3s5F8RnkzlnYEflVdmUDYWb5JNYUJWFZFzP3LGlBBWHs3WY5Tm2pN6Kft92XN1%2B2AUzVmbDqvk%2Fu0u%2Fnfz1ZrFfz3PHmffrbcrx60PI0bO2EYqYcpmKT8CI%2Bc2oWTxJ6QTlDe2uGA1xQw9LWCct11xFHBZOcBPmC7FAA%2BIDx9jOOM3IfzSIx%2BFZAyKhogytK7cXLzAAyIzxOiGN5lWdBwJha529gChkEngyfcSsPMCcR0KOEy2Q%2B80xg%2BE0k5RoFdX4EFNV9TKEbHID2ckWeay238QYxsJs1AKtvhym1R7lgolJilKIcM%2FVaxX9GqragX7lSju90gw18K2w5X0E2GHQB2AvLxkHEi6DbYP%2FQ6z%2BL3g%2Blu2HI2d0Q2gmJjCcuBafWE5qCvtoHYHLPeC5bAXLD%2BiBCfGUOwG9rWn6M%2BxjOE4aovjcMBxfziWPGYYx0%2BI%2FsQFKwckMwVnPcAIXc8QmN2GQO%2Bgegcw9wJmux9ShhMZNJsB8hVQo4sAmGJlmQoeB7I9ALlHIPcTKd%2FwYwxLWi7%2BOgZDDTXQAJaxSMNtGzG7Q8TcJ6b7iZj1qkaC%2BJ9q%2BGEO4XowbZC22wbT7hBM9whxT9gfJbX6ZR3zZEljdGCwhjyIkuU8QYlwB0LZlEzIHKa%2FE7IQTlPYTlRs4ZJ7gHNbcSk0T27y%2Bis%2FJdyapeQLzt%2ByGDJ7RSyeypPiCR8RxVwviOaOh%2BcTcZEVjrnnIh9UPAIoTteYfZdPwI9%2F5M24r5Rnd2vZKz%2FZiJPy3Zl0%2F70a9xuhWcdhI%2BbaQ8r3a5mynlwIdZX9DGCrVUjAB8KLDO0hywoS%2BMQ%2FmfPjmA%2BGaC7IFmWp%2FgWvc4jdvnBYjElK%2BMW7OZnnY2aMklekCSnK8Bt8Lp4gt9xCYmI7%2BuNOcksJg6zSGqYpWd3INYGif67BQqfe7ci7y4G%2BZCme8%2FvK1QTQGSWBBl7XK311y5uida8hcqnbmL8fU43ZZOtGWzXYtAaEXIM4humNuDDDSVLMEyr71PT%2F27cxTGB%2B23cUrZy6PXzNHiDQ7eGZsod%2FgL4HnzPnc8Cy1QTYNxZJ%2Ba1KBp%2FV5biV%2BfiGPM0LVSO4kTEjtFrwHPysaz8La1wKLGM1U79hUfZyPO3b%2FZxRNDlvcfy4r9mRrdnBM2YHkTt2k6TUSyhdJilTmL6MMY0LE6mpyi7H8LeAkEks0FMZmYHI4zYZCNcF3RSdriPgSEHeE1xzz5GCXefiTOldIYdTkpoANBLAe5MarlS4qTQQZLQ35%2FF8R5uEPQ185Yj7eutxs6vHaXsypoahtMV2PRoQyaip1Euu7Q9pfWNav3OkH6Ndkv%2ButN6QB7QBqY4tw2l9MKQYHxL6eJ4e%2BnjGFiSCi84xxlPIDEU9jqcu2JsrmAdOhxQul7IuJei5jkIl7vHl6Z7Ah580Ry69BSR7Q4HI0wk7stvVYWtDbctGEnx6xG06qmiVDw3M3zXzOyDS7G6MdPwOSadhJe9Tk84JmVbboDDsJyh0gTYr6cRgOiQMLmeu%2BohVxGunm9ns0wDO8Y%2FMRKYB12pnxjATdT4Thb3NRF3W%2FBr2tFYYQZiuggeVbyplNU8pqh0rqDVWG6yDvt%2FSz%2BW24aN%2BXjWvtNWxzSyHbNpMByBS6SC0z6yb2dpHKoH%2BkYrhCFeuHxne8fWeyvwZm3EN7lwM224rD%2BsuOGzrMratK7S7ZE%2F7EHs%2BpyR%2BzT0BZtOCMsGBcBzYB4n2DMLckXOVmitMvYecuybaY1AC%2FjFWPXWxQ1vrPm2tA1ha7%2FBcxvaPDGSasVstxH%2FWSqXZJVptVo4sUwu0YZe7SBtmik9dNjg5uvvgRVKNNyL1e%2F8jvOHYejUTgHPLmXsepDfm6LTWVZ8jL7xocfoaqemiA3A9vZKux7WXAr1Wu2aH8kXnuzRdPTEyVr8Iww7ZpeFHP%2F4%2FEfi7SppGI3DHVsvu8gOLliF4oNU69PCs9bKg9hi1rceG2SwaNoR8CJvZrlpdMLgTVu717CZW%2BnV3YPYTCLkWODKNtacOfULUIWSaO7rc5WI4c7yI7w9NY8%2BWgc0vgL3hG44P2s7SwbzFT3e%2FeVgiYvfTkc79fw%3D%3D%3C%2Fdiagram%3E%3C%2Fmxfile%3E)
