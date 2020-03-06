import React from 'react';
import { render } from '@testing-library/react';
import App from './App';

test('renders Results header', () => {
  const { getByText } = render(<App />);
  const linkElement = getByText(/Results/i);
  expect(linkElement).toBeInTheDocument();
});
