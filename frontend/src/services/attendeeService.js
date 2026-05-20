import httpClient from '../api/httpClient.js';

function getApiErrorMessage(error, fallback) {
  return (
    error?.response?.data?.errors?.[0] ||
    error?.response?.data?.message ||
    error?.message ||
    fallback
  );
}

export async function getMyEventRegistration(eventId) {
  try {
    const response = await httpClient.get(`/events/${eventId}/attendees/me`);
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to get registration status');
    }
    return response.data.data;
  } catch (error) {
    if (error?.response?.status === 404) {
      throw new Error('Attendee status endpoint not found (404). Please restart backend so new attendee endpoints are loaded.');
    }
    throw new Error(getApiErrorMessage(error, 'Failed to get registration status'));
  }
}

export async function joinEvent(eventId) {
  try {
    const response = await httpClient.post(`/events/${eventId}/attendees/me`, {});
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to join event');
    }
    return response.data.data;
  } catch (error) {
    if (error?.response?.status === 404) {
      // Compatibility fallback for environments that only expose /attendees.
      try {
        const alt = await httpClient.post(`/events/${eventId}/attendees`, {});
        if (!alt.data.success) {
          throw new Error(alt.data.message || 'Failed to join event');
        }
        return alt.data.data;
      } catch (fallbackError) {
        if (fallbackError?.response?.status === 404) {
          throw new Error('Join endpoint not found (404). Please restart backend so new attendee endpoints are loaded.');
        }
        throw new Error(getApiErrorMessage(fallbackError, 'Failed to join event'));
      }
    }
    throw new Error(getApiErrorMessage(error, 'Failed to join event'));
  }
}

export async function requestMyCheckIn(eventId, payload = {}) {
  try {
    const response = await httpClient.post(`/events/${eventId}/attendees/me/check-in-request`, payload);
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to request check-in');
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to request check-in'));
  }
}

export async function reviewAttendeeCheckIn(attendeeId, approve, note) {
  try {
    const response = await httpClient.put(`/attendees/${attendeeId}/check-in-review`, {
      approve: !!approve,
      note: note || undefined,
    });
    if (!response.data.success) {
      throw new Error(response.data.message || 'Failed to review check-in');
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error, 'Failed to review check-in'));
  }
}
