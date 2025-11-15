using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventsService.Aplicacion.Commands.CrearEvento;
using EventsService.Dominio.Excepciones;
using EventsService.Dominio.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EventsService.Test.Aplication.Commands.Evento
{
    public class CreateEventHandlerTest
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<ICategoryRepository> _categoryRepository;
        private readonly Mock<IScenarioRepository> _escenarioRepository;
        private readonly CreateEventHandler _handler;
        private readonly CreateEventCommand _request;
        private readonly Guid _idCategoria;
        private readonly Guid _idEscenario;


        public CreateEventHandlerTest()
        {
            _eventRepository = new Mock<IEventRepository>();
            _categoryRepository = new Mock<ICategoryRepository>();
            _escenarioRepository = new Mock<IScenarioRepository>();
            _handler = new CreateEventHandler(_eventRepository.Object, _categoryRepository.Object,
                _escenarioRepository.Object);

            _idCategoria = Guid.NewGuid();
            _idEscenario = Guid.NewGuid();

            _request = new CreateEventCommand(
                Nombre: "Prueba Test",
                AforoMaximo: 500,
                CategoriaId: _idCategoria,
                EscenarioId: _idEscenario,
                Inicio: DateTime.UtcNow.AddDays(10),
                Fin: DateTime.UtcNow.AddDays(11),
                Tipo: "Concierto",
                Lugar: "Sala Principal",
                Descripcion: "Descripción test"
            );
        }

        [Fact]
        public async Task Handle_CommandValido_Insert()
        {
            _categoryRepository.Setup(c => c.ExistsAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _escenarioRepository.Setup(e => e.ExistsAsync(_idEscenario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventRepository
                .Setup(ev => ev.InsertAsync(It.IsAny<Dominio.Entidades.Evento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(_request, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result);


        }

        [Fact]
        public async Task Handle_CommandValido_llamadoCorrecto()
        {
            _categoryRepository.Setup(c => c.ExistsAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _escenarioRepository.Setup(e => e.ExistsAsync(_idEscenario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _eventRepository
                .Setup(ev => ev.InsertAsync(It.IsAny<Dominio.Entidades.Evento>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(_request, CancellationToken.None);

            _eventRepository.Verify(x => x.InsertAsync(It.IsAny<Dominio.Entidades.Evento>(), It.IsAny<CancellationToken>()), Times.Once);


        }

        [Fact]
        public async Task Handle_CategoriaNoExiste_Fail()
        {
            _categoryRepository.Setup(c => c.ExistsAsync(_idCategoria, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(_request, CancellationToken.None));
        }


        [Fact]
        public async Task Handle_EscenarioNoExiste_Fail()
        {
            _escenarioRepository.Setup(e => e.ExistsAsync(_idEscenario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(_request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenCategoryMissing_ThrowsEventoExceptionWithMessage()
        {
            
            _categoryRepository.Setup(c => c.ExistsAsync(_request.CategoriaId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<EventoException>(() => _handler.Handle(_request, CancellationToken.None));
            Assert.Contains("categoría", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
