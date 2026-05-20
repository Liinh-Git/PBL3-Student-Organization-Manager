import httpClient from '../api/httpClient.js';

function getApiErrorMessage(responseData, fallbackMessage) {
  if (!responseData) return fallbackMessage;
  if (responseData.message) return responseData.message;
  if (Array.isArray(responseData.errors) && responseData.errors.length > 0) {
    return responseData.errors.join(', ');
  }
  return fallbackMessage;
}

export async function createOrganizationInvitation(orgId, payload) {
  try {
    const response = await httpClient.post(`/organizations/${orgId}/invitations`, payload);
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to create invitation'));
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to create invitation'));
  }
}

export async function createOrganizationInvitationRecommendation(orgId, payload) {
  try {
    const response = await httpClient.post(`/organizations/${orgId}/invitations/recommendations`, payload);
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to create recommendation'));
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to create recommendation'));
  }
}

export async function getMyInvitations() {
  const response = await httpClient.get('/users/me/invitations');
  if (!response.data.success) {
    throw new Error(getApiErrorMessage(response.data, 'Failed to get invitations'));
  }
  return Array.isArray(response.data.data) ? response.data.data : [];
}

export async function acceptMyInvitation(invitationId) {
  try {
    const response = await httpClient.post(`/users/me/invitations/${invitationId}/accept`, {});
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to accept invitation'));
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to accept invitation'));
  }
}

export async function rejectMyInvitation(invitationId) {
  try {
    const response = await httpClient.post(`/users/me/invitations/${invitationId}/reject`, {});
    if (!response.data.success) {
      throw new Error(getApiErrorMessage(response.data, 'Failed to reject invitation'));
    }
    return response.data.data;
  } catch (error) {
    throw new Error(getApiErrorMessage(error?.response?.data, error?.message || 'Failed to reject invitation'));
  }
}
