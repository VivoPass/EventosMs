using EventsService.Dominio.Entidades;
using EventsService.Dominio.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventsService.Infrastructura.mongo
{
    public sealed class EventCollections
    {
        public IMongoCollection<Evento> Eventos { get; }
        public IMongoCollection<Categoria> Categorias { get; }
        public IMongoCollection<Escenario> Escenarios { get; }

        public EventCollections(IMongoDatabase db)
        {
            Eventos = db.GetCollection<Evento>("eventos");
            Categorias = db.GetCollection<Categoria>("categorias");
            Escenarios = db.GetCollection<Escenario>("escenarios");
        }
    }


}