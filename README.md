# Metricas
Recebe na fila um nome de arquivo e faz o log dele além de analisar o tempo a cada 30 minutos

# Inicialização
Use o comando "docker-compose up" para subir o rabbitmq e a aplicação

Obs: o compose não está respeitando a hierarquia de dependencia que a aplicação tem do rabbitmq, então depois de subir o rabbitmq é preciso dar um restart na aplicação