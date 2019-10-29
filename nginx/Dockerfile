FROM nginx
RUN rm -v /etc/nginx/nginx.conf
ADD nginx.conf /etc/nginx
COPY certs /etc/nginx/certs
CMD service nginx start
