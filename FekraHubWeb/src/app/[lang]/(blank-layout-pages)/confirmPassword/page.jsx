import ConfirmPassword from '../../../../views/ConfirmPassword'

// Server Action Imports
import { getServerMode } from '@core/utils/serverHelpers'

export const metadata = {
  title: 'ConfirmPassword',
  description: 'Confirm your Password'
}

const ConfirmPage = () => {
  // Vars
  const mode = getServerMode()

  return <ConfirmPassword mode={mode} />
}

export default ConfirmPage
