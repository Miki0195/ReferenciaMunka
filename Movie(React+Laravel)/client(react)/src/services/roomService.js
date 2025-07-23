import api from './api';

const RoomService = {
  getAllRooms: async () => {
    try {
      const response = await api.get('/rooms');
      
      const rooms = response.data.data || response.data || [];
      
      const formattedRooms = Array.isArray(rooms) 
        ? rooms.map(room => ({
            ...room,
            rows: room.rows ? parseInt(room.rows) : 0,
            seats_per_row: room.seats_per_row ? parseInt(room.seats_per_row) : 0
          }))
        : [];
        
      return formattedRooms;
    } catch (error) {
      console.error('Room service error:', error);
      throw error.response?.data || error;
    }
  },

  getRoom: async (id) => {
    try {
      const response = await api.get(`/rooms/${id}`);
      const room = response.data.data || response.data;
      return {
        ...room,
        rows: room.rows ? parseInt(room.rows) : 0,
        seats_per_row: room.seats_per_row ? parseInt(room.seats_per_row) : 0
      };
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  createRoom: async (roomData) => {
    try {
      const response = await api.post('/rooms', roomData);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  updateRoom: async (id, roomData) => {
    try {
      const response = await api.put(`/rooms/${id}`, roomData);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  },

  deleteRoom: async (id) => {
    try {
      const response = await api.delete(`/rooms/${id}`);
      return response.data.data || response.data;
    } catch (error) {
      throw error.response?.data || error;
    }
  }
};

export default RoomService; 