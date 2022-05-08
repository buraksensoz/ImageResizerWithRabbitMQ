Adding images via rabbitMQ with background service

Firstly Run Rabbitmq service with docker or change your rabbitmq link from appsetting.json


docker cli: 
docker run -d -p 5672:5672 -p 15672:15672 --name rabbitmqcontainer rabbitmq:3.9.16-management
