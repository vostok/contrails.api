.PHONY: build

default: build

build:
	docker build -t skbkontur/contrails.api:latest .
