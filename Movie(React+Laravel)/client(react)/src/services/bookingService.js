import api from './api';

const BookingService = {
  getAllBookings: async () => {
    try {
      const response = await api.get('/bookings');
      console.log('API Response for bookings:', response);
      
      
      const bookings = response.data.data || response.data || [];
      return Array.isArray(bookings) ? bookings : [];
    } catch (error) {
      console.error('Booking service error:', error);
      throw error.response?.data || error;
    }
  },

  getBooking: async (id) => {
    try {
      const response = await api.get(`/bookings/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  createBooking: async (bookingData) => {
    try {
      
      if (!bookingData.screening_id) {
        throw new Error('Screening ID is required');
      }

      if (!bookingData.seats || !Array.isArray(bookingData.seats) || bookingData.seats.length === 0) {
        throw new Error('At least one seat must be selected');
      }

      if (!bookingData.ticket_types || !Array.isArray(bookingData.ticket_types) || bookingData.ticket_types.length === 0) {
        throw new Error('At least one ticket type must be selected');
      }
      
      const formattedSeats = bookingData.seats.map(seat => ({
        row: parseInt(seat.row),
        number: parseInt(seat.seat || seat.number)
      }));

      
      const formattedTicketTypes = bookingData.ticket_types.map(ticket => ({
        type: ticket.type,
        quantity: parseInt(ticket.quantity || ticket.count || 1)
      }));

      
      const formattedData = {
        screening_id: parseInt(bookingData.screening_id),
        seats: formattedSeats,
        ticket_types: formattedTicketTypes
      };

      console.log('Sending booking data:', formattedData);
      
      const response = await api.post('/bookings', formattedData);
      console.log('Booking response:', response);
      
      if (response.status >= 400) {
        throw new Error(response.data?.message || 'Booking failed');
      }
      
      return response.data.data || response.data;
    } catch (error) {
      console.error('Booking creation error:', error);
      throw error.response?.data || error;
    }
  },

  cancelBooking: async (id) => {
    try {
      const response = await api.put(`/bookings/${id}`, { status: 'cancelled' });
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  deleteBooking: async (id) => {
    try {
      const response = await api.delete(`/bookings/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  }
};

export default BookingService; 