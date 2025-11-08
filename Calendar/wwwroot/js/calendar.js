document.addEventListener('DOMContentLoaded', function() {
        const CalendarObj = window.tui.Calendar
        const calendar = new CalendarObj('#calendar', {
            defaultView: 'week',
            isReadOnly: false,
            week: {
                startDayOfWeek: 1,  
                hourStart: 8,
                hourEnd: 22,
                taskView: false,   
                scheduleView: ['time'], 
                
            },
            calendars: [{ id: '1', name: 'Tareas', backgroundColor: '#03a9f4' }]
        });

        const now = new Date();
        const eventos = [
            {
                id: '1',
                calendarId: '1',
                title: 'Revisar documentación',
                start: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 10, 0),
                end: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 11, 30)
            },
            {
                id: '2',
                calendarId: '1',
                title: 'Desarrollar módulo API',
                start: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 15, 0),
                end: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 17, 0)
            }
        ];
        calendar.createEvents(eventos);
    });