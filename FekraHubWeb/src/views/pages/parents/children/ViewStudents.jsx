// MUI Imports
import Link from 'next/link'

import { useParams } from 'next/navigation'

import Grid from '@mui/material/Grid'
import Button from '@mui/material/Button'

// Component Imports
import ChildrenCard from '@components/children-card/card'
import { getLocalizedUrl } from '@/utils/i18n'

// // Vars
const data = [
  {
    firstName: 'Sulaf',
    lastName: 'AlManna',
    avatarIcon: 'ri-group-line',
    avatarColor: 'primary',
    course: 'Class1',
    sDate: '03-03-2024',
    eDate: '08-03-2024',
    pCourse: '100$'
  },
  {
    firstName: 'Maher',
    lastName: 'ALMalek',
    avatarIcon: 'ri-group-line',
    avatarColor: 'primary',
    course: 'Class4',
    sDate: '03-03-2024',
    eDate: '08-03-2024',
    pCourse: '100$'
  },
  {
    firstName: 'Samer',
    lastName: 'Sugar',
    avatarIcon: 'ri-group-line',
    avatarColor: 'primary',
    course: 'Class7',
    sDate: '03-03-2024',
    eDate: '08-03-2024',
    pCourse: '100$'
  },
  {
    firstName: 'Nahed',
    lastName: 'AlHalbi',
    avatarIcon: 'ri-group-line',
    avatarColor: 'primary',
    course: 'Class6',
    sDate: '03-03-2024',
    eDate: '08-03-2024',
    pCourse: '100$'
  },
  {
    firstName: 'Nahed',
    lastName: 'AlHalbi',
    avatarIcon: 'ri-group-line',
    avatarColor: 'primary',
    course: 'Class6',
    sDate: '03-03-2024',
    eDate: '08-02-2024',
    pCourse: '100$'
  }
]

const UserListCards = () => {
  return (
    <div>
      <Button
        variant='contained'
        component={Link}
        startIcon={<i className='ri-add-line' />}
        href={getLocalizedUrl('pages/parents/children/create-student', 'en')}
        className='is-full sm:is-auto'
      >
        Create Student
      </Button>

      <Grid container spacing={6}>
        {data.map((item, i) => (
          <Grid key={i} item xs={12} sm={6} md={3}>
            <ChildrenCard {...item} />
          </Grid>
        ))}
      </Grid>
    </div>
  )
}

export default UserListCards
