import api from './api';

const TicketTypeService = {
  getAllTicketTypes: async () => {
    try {
      return [
        { name: 'normal', price_multiplier: 1.00, display_name: 'Normál jegy' },
        { name: 'student', price_multiplier: 0.75, display_name: 'Diák jegy' },
        { name: 'senior', price_multiplier: 0.80, display_name: 'Nyugdíjas jegy' }
      ];
    } catch (error) {
      console.error('Ticket type service error:', error);
      throw error.response?.data || error;
    }
  }
};

export default TicketTypeService; 