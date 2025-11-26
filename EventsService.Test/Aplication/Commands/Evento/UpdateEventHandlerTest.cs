//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EventsService.Aplicacion.Commands.CrearEvento;
//using EventsService.Aplicacion.Commands.ModificarEvento;
//using EventsService.Dominio.Interfaces;
//using Moq;

//namespace EventsService.Test.Aplication.Commands.Evento
//{
//    public class UpdateEventHandlerTest
//    {
//        private readonly Mock<IEventRepository> _eventRepository;
//        private readonly Mock<ICategoryRepository> _categoryRepository;
//        private readonly Mock<IScenarioRepository> _escenarioRepository;
//        private readonly UpdateEventHandler _handler;
//        private readonly UpdateEventCommand _command;
//        private readonly Guid _idEvento;
//        private readonly Guid _idCategoria;
//        private readonly Guid _idEscenario;
//        private readonly Dominio.Entidades.Evento _existing;


//        public UpdateEventHandlerTest()
//        {
//            _eventRepository = new Mock<IEventRepository>();
//            _categoryRepository = new Mock<ICategoryRepository>();
//            _escenarioRepository = new Mock<IScenarioRepository>();

//            _idEvento = Guid.NewGuid();
//            _idCategoria = Guid.NewGuid();
//            _idEscenario = Guid.NewGuid();


//            _handler = new UpdateEventHandler(_eventRepository.Object, _categoryRepository.Object,
//                _escenarioRepository.Object);

//            _command = new UpdateEventCommand(
//                Id: _idEvento,
//                Nombre: "Nuevo nombre del evento",
//                CategoriaId: _idCategoria,
//                EscenarioId: _idEscenario,
//                Inicio: DateTimeOffset.UtcNow.AddDays(1),
//                Fin: DateTimeOffset.UtcNow.AddDays(2),
//                AforoMaximo: 5000,
//                Tipo: "Concierto",
//                Lugar: "Sala Principal",
//                Descripcion: "Evento actualizado para pruebas unitarias"
//            );


//            _existing = new Dominio.Entidades.Evento
//            {
//                Id = _command.Id,
//                Nombre = "Nombre Antiguo",
//                CategoriaId = Guid.NewGuid(),
//                EscenarioId = Guid.NewGuid(),
//                Inicio = DateTimeOffset.UtcNow,
//                Fin = DateTimeOffset.UtcNow.AddHours(2),
//                AforoMaximo = 100,
//                Tipo = "Old",
//                Lugar = "Lugar A",
//                Descripcion = "Desc antigua"
//            };

//        }


//        [Fact]
//        public async Task ModificarEvento_RetornaTrue()
//        {
//            _eventRepository.Setup(r => r.GetByIdAsync(_command.Id, It.IsAny<CancellationToken>()))
//                .ReturnsAsync(_existing);

//            _categoryRepository.Setup(x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);
//            _escenarioRepository.Setup(s => s.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            _eventRepository
//                .Setup(r => r.UpdateAsync(It.IsAny<Dominio.Entidades.Evento>(), It.IsAny<CancellationToken>()))
//                .ReturnsAsync(true);

//            var result = await _handler.Handle(_command, CancellationToken.None);

//            Assert.True(result);

//        }
//    }
//}
